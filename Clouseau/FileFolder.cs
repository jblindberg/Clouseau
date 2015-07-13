/*
 * Copyright (c) 2015 Jeff Lindberg
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Clouseau
{
	public class FileFolderInstance : AbstractInstance, FileInstanceInfo
	{
	    /// <summary>
		/// Relative path to the file
		/// </summary>
		public virtual string Location { get; set; }

		/// <summary>
		/// Full path to file
		/// </summary>
		public virtual string Path { get; set; }

		/// <summary>
		/// File name with extension
		/// </summary>
		public virtual string FileName { get; set; }

		/// <summary>
		/// User for remote UNC access
		/// </summary>
		public virtual string User { get; set; }

		/// <summary>
		/// Password for remote UNC access
		/// </summary>
		public virtual string Password { get; set; }


		public virtual long FileSize { get; set; }


		/// <summary>
		/// Returns path to file.
		/// </summary>
		public override string ContentName { get { return Location; } }


		/// <summary>
		/// Just return the info that isn't available in another standard property
		/// </summary>
		public override string AdditionalSummary
		{
			get
			{
				return string.Format("{0:00.0} MB", Util.SizeInMb(FileSize));
			}
		}


		/// <summary>
		/// Simple multi-line listing of item content, with newlines separating lines
		/// </summary>
		public override string Details
		{
			get
			{
				return string.Format("{0} {1} : {2}\r\n{3}\r\n{4:0.0} MB ({6})\r\nCreated {7}\r\nUpdated {5}", 
					EntityName, ID, Status, Location, Util.SizeInMb(FileSize), UpdateDate, FileSize, ArrivalDate);
			}
		}



	}


	public struct Folder
	{
		public string Name;
		public string Status;
		public string Path;

		public Folder(string name, string status, string path)
		{
			this.Name = name;
			this.Status = status;
			this.Path = path;
		}
	}

	/// <summary>
	/// Station based on files in configured directories (corresponding to states) and extensions.
	/// Note that certain methods can be overridden when subclasses are implemented.
	/// </summary>
	public class FileFolderStation : AbstractStation
	{

		protected string HomePath;
		protected string User;
		protected string Password;
		protected string EntityName;
		protected bool UseUnc;
		protected bool IncludeSubfolders;

		protected List<Folder> Folders = new List<Folder>();

		// Extensions include the preceding "." (e.g. ".txt"), and are case insensitive
		protected List<string> Extensions = new List<string>();


        /// <summary>
        /// initialization using data in ConfigData object.
        /// This should be called immediately after the constructor.
        /// Each Station subclass should call base.initialize(configData, memory) from its  own initialize() method.
        /// </summary>
        public override void Initialize(ConfigData configData, InstanceMemory memory, Resolver commandResolver)
		{
			base.Initialize(configData, memory, commandResolver);

			// do any other Station specific initialization here

			HomePath = configData.RequiredValue("homePath");
			if (!(HomePath.EndsWith(@"\") || HomePath.EndsWith(@"/")))
				HomePath += "/";

			User = configData.Value("user");
			Password = configData.Value("password");

			IncludeSubfolders = configData.BoolValue("includeSubfolders", false);

			string possibleError;
			var unc = ConnectUnc(out possibleError);

			try
			{

				EntityName = configData.RequiredValue("entityName");


				List<ConfigData> folderConfigs = configData.GetConfigSections("folder");
				if (folderConfigs.Count == 0)
					throw new Exception("No folders configured for station " + this.GetType());
				foreach (ConfigData folderConfig in folderConfigs)
				{
					string name = folderConfig.RequiredValue("name");
					string status = folderConfig.Value("status");
					if (status == null) status = name;
					string path = HomePath + name;

					if (Directory.Exists(path))
					{
						Folder f = new Folder(name, status, path);
						Folders.Add(f);
					}
					else
					{
						Console.Error.WriteLine("WARNING: Directory not found: {0}", path);
					}
				}

				if (Folders.Count == 0)
				{
					string error = "No folders accessible for station " + this.GetType();
					if (possibleError != null)
						error += "; " + possibleError;
					throw new Exception(error);
				}

				List<ConfigData> extConfigs = configData.GetConfigSections("extension");
				foreach (ConfigData extConfig in extConfigs)
				{
					string ext = extConfig.Value();
					if (string.IsNullOrEmpty(ext))
					{
						throw new Exception("Property is empty: extension");
					}
					Extensions.Add(ext);
				}

			}
			finally
			{
				if (unc != null) unc.Dispose();
			}

		}

		private FileSystemConnection ConnectUnc(out string possibleError)
		{
			var unc = FileSystemConnectionManager.Instance.GetConnection(User, Password, HomePath);
			possibleError = (unc != null) ? unc.ErrorMessage : null;
			//UNCAccessWithCredentials unc = Util.ConnectUNC(user, password, homePath, out possibleError, out useUNC);
			return unc;
		}


        /// <summary>
        /// Retrieve any instances of objects at this Station with the specified criteria.
        /// </summary>
        /// <returns>Set of instances found at this Station</returns>
        public override InstanceRefList DoSearch(ICollection<Criterion> crit)
		{

			InstanceRefList results = new InstanceRefList(this);

			string possibleError;
			var unc = ConnectUnc(out possibleError);

			try
			{
				// build extension filter
				Expression<Func<FileInfo, bool>> extFilter = null;
				if (Extensions.Count > 0)
				{
					extFilter = PredicateBuilder.False<FileInfo>();
					foreach (string ext in Extensions)
					{
						// The temporary variable in the loop is required to avoid the outer variable trap, 
						// where the same variable is captured for each iteration of the foreach loop.
						string temp = ext;
						extFilter = extFilter.Or(f => temp.Equals(f.Extension, StringComparison.InvariantCultureIgnoreCase));
					}
				}

				foreach (Folder folder in Folders)
					if (IncludeFolder(folder, crit))
					{
						if (results.List.Count >= MaxSearchResults)
							break;

						SearchFolder(crit, folder, extFilter, results);

						if (IncludeSubFolders(folder))
						{
							List<Folder> subFolders = GetSubFolders(folder);
							foreach (var subFolder in subFolders)
							{
								if (results.List.Count >= MaxSearchResults)
									break;

								SearchFolder(crit, subFolder, extFilter, results);					            
							}
						}
					}
			}
			finally
			{
				if (unc != null) unc.Dispose();
			}
			return results;
		}

		/// <summary>
		/// Should we search subfolders?
		/// </summary>
		/// <param name="folder">parent folder in question</param>
		/// <returns></returns>
		protected virtual bool IncludeSubFolders(Folder folder)
		{
			return IncludeSubfolders;
		}

		/// <summary>
		/// Get the relevant subfolders in the specified parent folder.
		/// Includes recursively nested folders.
		/// </summary>
		/// <param name="folder"></param>
		/// <returns></returns>
		protected virtual List<Folder> GetSubFolders(Folder folder)
		{
			var list = new List<Folder>();
			AddSubFolders(folder, list, null);
			return list;
		}

		private void AddSubFolders(Folder folder, List<Folder> list, string parentName)
		{
			DirectoryInfo parent = new DirectoryInfo(folder.Path);
			foreach (var subdir in parent.EnumerateDirectories())
			{
				string prefix = (parentName != null) ? parentName + "/" : "";
				string name = prefix + subdir.Name;
				var child = new Folder(name, folder.Status, subdir.FullName);
				list.Add(child);
				AddSubFolders(child, list, name); // recursion to nested folders
			}
		}

		/// <summary>
		/// Search one folder, add to results
		/// </summary>
		/// <param name="crit"></param>
		/// <param name="folder"></param>
		/// <param name="extFilter"></param>
		/// <param name="results"></param>
		protected virtual void SearchFolder(ICollection<Criterion> crit, Folder folder, Expression<Func<FileInfo, bool>> extFilter, InstanceRefList results)
		{
			DirectoryInfo dir = new DirectoryInfo(folder.Path);

			var query = (from file in dir.EnumerateFiles() select file).AsQueryable();

			if (extFilter != null)
			{
				query = query.Where(extFilter);
			}

			query = ApplyFileInfoCriteria(crit, query, folder);

			foreach (FileInfo f in query.ToList()) // TODO speed up; taking 14% of search time
			{
				if (results.List.Count >= MaxSearchResults)
					break;

				if (Valid(f, crit)) // apply other criteria that need to be done on the file content
				{
					FileFolderInstance i = CreateInstance();

					// populate properties

					// Method to be overridden in subclass.  Default = filename without extension.
					i.ID = MapFileToId(f, folder);

					i.EntityName = EntityName;
					i.FileName = f.Name;
					i.Status = MapFileToStatus(f, folder);
					i.UniqueId = MapFileToUniqueId(f, folder);
					i.UpdateDate = f.LastWriteTime;
					i.ArrivalDate = f.CreationTime;
					i.Location = folder.Name + "/" + f.Name;
					i.Path = folder.Path + "/" + f.Name;
					i.FileSize = f.Length;
					i.User = User;
					i.Password = Password;

					PopulateCustomProperties(i);

					InstanceRef iref = Memory.AddRef(i, this);
					results.AddInstanceRef(iref);
				}
			}
		}

		/// <summary>
		/// Can be overridden to populate additional fields of a subclass
		/// </summary>
		/// <param name="i"></param>
		protected virtual void PopulateCustomProperties(FileFolderInstance i)
		{
			// default case is nothing
		}

		/// <summary>
		/// Can be overridden to create instances of alternate subclasses
		/// </summary>
		protected virtual FileFolderInstance CreateInstance()
		{
			FileFolderInstance i = new FileFolderInstance();
			return i;
		}

		protected virtual IQueryable<FileInfo> ApplyFileInfoCriteria(ICollection<Criterion> crit, IQueryable<FileInfo> query, Folder folder)
		{
			foreach (Criterion c in crit)
			{

				if (c.FieldName == Field.EntityId)
				{
					query = ApplyRequestIdCriteria(query, c);
				}
				else if (c.FieldName == Field.InstanceUpdateDate)
				{
					query = ApplyUpdateDateCriteria(query, c);
				}
				else if (c.FieldName == Field.InstanceArrivalDate)
				{
					query = ApplyArrivalDateCriteria(query, c);
				}
				else if (c.Operation == Criterion.Stuck)
				{
					query = ApplyStuckCriteria(query, c);
				}

			}
			return query;
		}

		protected virtual IQueryable<FileInfo> ApplyStuckCriteria(IQueryable<FileInfo> query, Criterion c)
		{
			TimeSpan oldAge = new TimeSpan(0, 0, AgeLimit);
			query = query.Where(f => DateTime.Now - f.LastWriteTime > oldAge);
			return query;
		}

		protected virtual IQueryable<FileInfo> ApplyUpdateDateCriteria(IQueryable<FileInfo> query, Criterion c)
		{
			DateTime d = DateTime.Parse(c.Value);

			if (c.Operation == Criterion.GreaterThan)
				query = query.Where(f => f.LastWriteTime > d);
			else if (c.Operation == Criterion.GreaterThanOrEqual)
				query = query.Where(f => f.LastWriteTime >= d);
			else if (c.Operation == Criterion.LessThan)
				query = query.Where(f => f.LastWriteTime < d);
			else if (c.Operation == Criterion.LessThanOrEqual)
				query = query.Where(f => f.LastWriteTime <= d);
			else
			{
				throw new InvalidOperationException(string.Format("Can't use {0} operator with Update Date ", c.Operation));
			}
			return query;
		}

		protected virtual IQueryable<FileInfo> ApplyArrivalDateCriteria(IQueryable<FileInfo> query, Criterion c)
		{
			DateTime d = DateTime.Parse(c.Value);

			if (c.Operation == Criterion.GreaterThan)
				query = query.Where(f => f.CreationTime > d);
			else if (c.Operation == Criterion.GreaterThanOrEqual)
				query = query.Where(f => f.CreationTime >= d);
			else if (c.Operation == Criterion.LessThan)
				query = query.Where(f => f.CreationTime < d);
			else if (c.Operation == Criterion.LessThanOrEqual)
				query = query.Where(f => f.CreationTime <= d);
			else
			{
				throw new InvalidOperationException(string.Format("Can't use {0} operator with Update Date ", c.Operation));
			}
			return query;
		}

		protected virtual IQueryable<FileInfo> ApplyRequestIdCriteria(IQueryable<FileInfo> query, Criterion c)
		{
			string id = c.Value;

			if (c.Operation == Criterion.Equal)
				query = query.Where(f => CompareFileId(f, id) == 0);
			else if (c.Operation == Criterion.NotEqual)
				query = query.Where(f => CompareFileId(f, id) != 0);
			else if (c.Operation == Criterion.GreaterThan)
				query = query.Where(f => CompareFileId(f, id) > 0);
			else if (c.Operation == Criterion.GreaterThanOrEqual)
				query = query.Where(f => CompareFileId(f, id) >= 0);
			else if (c.Operation == Criterion.LessThan)
				query = query.Where(f => CompareFileId(f, id) < 0);
			else if (c.Operation == Criterion.LessThanOrEqual)
				query = query.Where(f => CompareFileId(f, id) <= 0);
			else
			{
				throw new InvalidOperationException(string.Format("Can't use {0} operator with ID based on file name", c.Operation));
			}
			return query;
		}

		/// <summary>
		/// Compares file basename to ID.
		/// If both id and file basename are integers, compare as integers, otherwise compare as strings.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="f"></param>
		/// <returns>Same return codes as String.Compare method</returns>
		public virtual int CompareFileId(FileInfo f, string id)
		{
			string fname = Util.Basename(f);
			return Util.CompareIntegerOrString(fname, id);
		}


		// Should we search this folder, given the criteria?
		// Default: assumes folder status is based on folder name, including mapping in config file
		// Only support equal and not equal operations for folder/status values.
		// Also considers folder status if we're doing a "STUCK" search.
		protected virtual bool IncludeFolder(Folder folder, ICollection<Criterion> crit)
		{
			// First check STUCK criteria (skip Final folders)
			if (Criterion.HasStuckCriterion(crit) && !CheckForStuck(folder))
				return false;

			// Now check STATUS criteria (maps to folders)
			var statusCrit = (from c in crit where c.FieldName == Field.InstanceStatus select c).ToList();

			var notEqualThisFolder = 
				(from c in statusCrit 
					where (c.Operation == Criterion.NotEqual && c.Value.Equals(folder.Status, StringComparison.InvariantCultureIgnoreCase)) 
					select c).Any();
			if (notEqualThisFolder)
				return false;

			bool hasEqual = (from c in statusCrit where c.Operation == Criterion.Equal select c).Any();
			bool equalThisFolder = 
				(from c in statusCrit
				 where (c.Operation == Criterion.Equal && c.Value.Equals(folder.Status, StringComparison.InvariantCultureIgnoreCase)) 
				 select c).Any();
			if (hasEqual && !equalThisFolder)
				return false;

			return true;
		}

		/// <summary>
		/// Is this a folder where we should check for Stuck entities?
		/// </summary>
		protected virtual bool CheckForStuck(Folder folder)
		{
			return folder.Status != Constants.StatusComplete && folder.Status != Constants.StatusArchive;
		}


		// Use folder status as the default status.
		protected virtual string MapFileToStatus(FileInfo f, Folder folder)
		{
			return folder.Status;
		}

		// Use basename of file as the default ID.
		protected virtual string MapFileToId(FileInfo f, Folder folder)
		{
			return Util.Basename(f);
		}

		// Use folder + filename as the default Unique ID.
		protected virtual string MapFileToUniqueId(FileInfo f, Folder folder)
		{
			return folder.Name + "/" + f.Name;
		}

		// Does this file actually meet the criteria, beyond those determined by the file name and path
		protected virtual bool Valid(FileInfo f, ICollection<Criterion> crit)
		{
			// Default: OK
			return true;
		}


		/// <summary>
		/// Return the actual file content
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override byte[] Content(Instance i)
		{
			string possibleError;
			var unc = ConnectUnc(out possibleError);

			try
			{
				FileFolderInstance ffi = (FileFolderInstance)i;
				byte[] content = File.ReadAllBytes(ffi.Path);
				return content;
				//Stream stream = new FileStream(ffi.Path, FileMode.Open, FileAccess.Read);
				//return stream;
			}
			finally
			{
				if (unc != null) unc.Dispose();
			}

		}


		public override ContentType ContentType(Instance i)
		{
			FileFolderInstance ffi = (FileFolderInstance)i;
			return Util.FileContentType(ffi.Location);
		}


		public override bool HasContent
		{
			get
			{
				return true;
			}
		}


		/// <summary>
		/// Delete the file corresponding to the specified FileFolderInstance
		/// </summary>
		/// <param name="instance">FileFolderInstance is expected; 
		///     method signature uses Instance in case we add this method to the Station interface,
		///     or to another station-agnostic interface</param>
		public virtual void Delete(Instance instance)
		{
			FileFolderInstance ffi = (FileFolderInstance)instance;
			this.DeleteFile(ffi.Path);
		}


		/// <summary>
		/// Delete the file at the specified path.  File is associated with this station.
		/// If needed, use UNC connection associated with this station for remote file systems.
		/// No exception is thrown if the file does not exist.
		/// </summary>
		/// <param name="path">Full path to file being deleted.</param>
		public virtual void DeleteFile(string path)
		{
			string possibleError;
			var unc = ConnectUnc(out possibleError);

			try
			{
				File.Delete(path);
			}
			finally
			{
				if (unc != null) unc.Dispose();
			}

		}



	}

}
