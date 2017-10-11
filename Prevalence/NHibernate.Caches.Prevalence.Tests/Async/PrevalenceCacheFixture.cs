﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


#region License

//
//  PrevalenceCache - A cache provider for NHibernate using Bamboo.Prevalence.
//
//  This library is free software; you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation; either
//  version 2.1 of the License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// CLOVER:OFF
//

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using log4net.Config;
using NHibernate.Cache;
using NUnit.Framework;

namespace NHibernate.Caches.Prevalence.Tests
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class PrevalenceCacheFixtureAsync
	{
		private PrevalenceCacheProvider provider;
		private Dictionary<string, string> props;

		[OneTimeSetUp]
		public void FixtureSetup()
		{
			XmlConfigurator.Configure();
			props = new Dictionary<string, string>
			{
				{
					"prevalenceBase",
					Path.Combine(Path.GetDirectoryName(typeof(PrevalenceCacheFixtureAsync).Assembly.Location), "CacheStorage")
				}
			};
			provider = new PrevalenceCacheProvider();
			provider.Start(props);
		}

		[OneTimeTearDown]
		public void FixtureTeardown()
		{
			provider.Stop();
		}

		[Serializable]
		private class SomeObject
		{
			public int Id;

			public override int GetHashCode()
			{
				return 1;
			}

			public override string ToString()
			{
				return "TestObject";
			}

			public override bool Equals(object obj)
			{
				var other = obj as SomeObject;

				if (other == null)
				{
					return false;
				}

				return other.Id == Id;
			}
		}

		[Test]
		public async Task TestClearAsync()
		{
			string key = "key1";
			string value = "value";

			ICache cache = provider.BuildCache("nunit", props);
			Assert.IsNotNull(cache, "no cache returned");

			// add the item
			await (cache.PutAsync(key, value, CancellationToken.None));

			// make sure it's there
			object item = await (cache.GetAsync(key, CancellationToken.None));
			Assert.IsNotNull(item, "couldn't find item in cache");

			// clear the cache
			await (cache.ClearAsync(CancellationToken.None));

			// make sure we don't get an item
			item = await (cache.GetAsync(key, CancellationToken.None));
			Assert.IsNull(item, "item still exists in cache");
		}

		[Test]
		public async Task TestNonEqualObjectsWithEqualHashCodeAndToStringAsync()
		{
			var obj1 = new SomeObject();
			var obj2 = new SomeObject();

			obj1.Id = 1;
			obj2.Id = 2;

			ICache cache = provider.BuildCache("nunit", props);

			Assert.IsNull(await (cache.GetAsync(obj2, CancellationToken.None)));
			await (cache.PutAsync(obj1, obj1, CancellationToken.None));
			Assert.AreEqual(obj1, await (cache.GetAsync(obj1, CancellationToken.None)));
			Assert.IsNull(await (cache.GetAsync(obj2, CancellationToken.None)));
		}

		[Test]
		public async Task TestNullKeyGetAsync()
		{
			ICache cache = provider.BuildCache("nunit", props);
			await (cache.PutAsync("nunit", "value", CancellationToken.None));
			object item = await (cache.GetAsync(null, CancellationToken.None));
			Assert.IsNull(item);
		}

		[Test]
		public void TestNullKeyPutAsync()
		{
			ICache cache = new PrevalenceCache();
			Assert.ThrowsAsync<ArgumentNullException>(() => cache.RemoveAsync(null, CancellationToken.None));
		}

		[Test]
		public void TestNullKeyRemoveAsync()
		{
			ICache cache = new PrevalenceCache();
			Assert.ThrowsAsync<ArgumentNullException>(() => cache.RemoveAsync(null, CancellationToken.None));
		}

		[Test]
		public void TestNullValuePutAsync()
		{
			ICache cache = new PrevalenceCache();
			Assert.ThrowsAsync<ArgumentNullException>(() => cache.PutAsync("nunit", null, CancellationToken.None));
		}

		[Test]
		public async Task TestPutAsync()
		{
			string key = "key1";
			string value = "value";

			ICache cache = provider.BuildCache("nunit", props);
			Assert.IsNotNull(cache, "no cache returned");

			Assert.IsNull(await (cache.GetAsync(key, CancellationToken.None)), "cache returned an item we didn't add !?!");

			await (cache.PutAsync(key, value, CancellationToken.None));
			object item = await (cache.GetAsync(key, CancellationToken.None));
			Assert.IsNotNull(item);
			Assert.AreEqual(value, item, "didn't return the item we added");
		}

		[Test]
		public async Task TestRegionsAsync()
		{
			string key = "key";
			ICache cache1 = provider.BuildCache("nunit1", props);
			ICache cache2 = provider.BuildCache("nunit2", props);
			string s1 = "test1";
			string s2 = "test2";
			await (cache1.PutAsync(key, s1, CancellationToken.None));
			await (cache2.PutAsync(key, s2, CancellationToken.None));
			object get1 = await (cache1.GetAsync(key, CancellationToken.None));
			object get2 = await (cache2.GetAsync(key, CancellationToken.None));
			Assert.IsFalse(get1 == get2);
		}

		[Test]
		public async Task TestRemoveAsync()
		{
			string key = "key1";
			string value = "value";

			ICache cache = provider.BuildCache("nunit", props);
			Assert.IsNotNull(cache, "no cache returned");

			// add the item
			await (cache.PutAsync(key, value, CancellationToken.None));

			// make sure it's there
			object item = await (cache.GetAsync(key, CancellationToken.None));
			Assert.IsNotNull(item, "item just added is not there");

			// remove it
			await (cache.RemoveAsync(key, CancellationToken.None));

			// make sure it's not there
			item = await (cache.GetAsync(key, CancellationToken.None));
			Assert.IsNull(item, "item still exists in cache");
		}
	}
}
