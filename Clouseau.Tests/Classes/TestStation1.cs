using System.Collections.Generic;
using System.Diagnostics;

namespace Clouseau.Tests
{
	class TestStation1 : AbstractStation
	{

		private static int itemNumber;  // for generating test data

		/**
		 * initialization using data in ConfigData object.
		 * This should be called immediately after the constructor.
		 * Each Station subclass should call base.initialize(configData, memory) from its
		 * own initialize() method.
		 */
		public override void Initialize(ConfigData configData, InstanceMemory memory, Resolver commandResolver)
		{
			base.Initialize(configData, memory, commandResolver);

		}

		/**
		 * Connect to the desired system so that the Station can obtain
		 * real-time document information.
		 */
		public override void Connect()
		{	}

		/**
		 * Shut down this Station, and release any connections or other resources.
		 */
		public override void Disconnect()
		{ }

		/**
		 * Retrieve any instances of objects at this Station with the specified
		 * criteria.
		 *
		 * @return       Set of instances found at this Station
		 */
		public override InstanceRefList DoSearch(ICollection<Criterion> crit)
		{
			string prefix = "TEST";
			foreach (Criterion c in crit)
			{
				prefix = c.Value; // pick up value arg from last criterion
			}
			return pretendSearch(prefix);
		}

		private InstanceRefList pretendSearch(string prefix)
		{
			Debug.WriteLine("pretendSearch: prefix=" + prefix);
			List<InstanceRef> resultsList = new List<InstanceRef>();
			int i;
			for (i = itemNumber; i < itemNumber + 5; i++)
			{
				Instance instance = new TestInstance(prefix + "_" + i, "99" + i);
				InstanceRef iref = Memory.AddRef(instance, this);
				resultsList.Add(iref);
			}
			itemNumber = i;
			InstanceRefList irl = new InstanceRefList(this);
			irl.List = (resultsList);
			return irl;
		}


	}
}
