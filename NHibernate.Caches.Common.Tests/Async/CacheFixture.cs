﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Cache;
using NUnit.Framework;

namespace NHibernate.Caches.Common.Tests
{
	public abstract partial class CacheFixture : Fixture
	{

		[Test]
		public async Task TestPutAsync()
		{
			const string key = "keyTestPut";
			const string value = "valuePut";

			var cache = GetDefaultCache();
			// Due to async version, it may already be there.
			await (cache.RemoveAsync(key, CancellationToken.None));

			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Null, "cache returned an item we didn't add !?!");

			await (cache.PutAsync(key, value, CancellationToken.None));
			var item = await (cache.GetAsync(key, CancellationToken.None));
			Assert.That(item, Is.Not.Null, "Unable to retrieve cached item");
			Assert.That(item, Is.EqualTo(value), "didn't return the item we added");
		}

		[Test]
		public async Task TestDistributedPutAsync()
		{
			if (!SupportsDistributedCache)
			{
				Assert.Ignore("Test not supported by provider");
			}

			const string key = "keyTestPut";
			const string value = "valuePut";

			var cache = GetDefaultCache();
			var cache2 = (CacheBase) GetNewProvider().BuildCache(DefaultRegion, GetDefaultProperties());
			var cache3 = (CacheBase) GetNewProvider().BuildCache(DefaultRegion, GetDefaultProperties());
			// Due to async version, it may already be there.
			await (cache.RemoveAsync(key, CancellationToken.None));
			await (Task.Delay(DistributedSynhronizationTime));
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Null, "cache returned an item we didn't add !?!");
			Assert.That(await (cache2.GetAsync(key, CancellationToken.None)), Is.Null, "cache returned an item we didn't add !?!");
			Assert.That(await (cache3.GetAsync(key, CancellationToken.None)), Is.Null, "cache returned an item we didn't add !?!");

			await (cache.PutAsync(key, value, CancellationToken.None));
			await (Task.Delay(DistributedSynhronizationTime));
			AssertItem(await (cache.GetAsync(key, CancellationToken.None)));
			AssertItem(await (cache2.GetAsync(key, CancellationToken.None)));
			AssertItem(await (cache3.GetAsync(key, CancellationToken.None)));

			void AssertItem(object item)
			{
				Assert.That(item, Is.Not.Null, "Unable to retrieve cached item");
				Assert.That(item, Is.EqualTo(value), "didn't return the item we added");
			}
		}

		[Test]
		public async Task TestRemoveAsync()
		{
			const string key = "keyTestRemove";
			const string value = "valueRemove";

			var cache = GetDefaultCache();

			// add the item
			await (cache.PutAsync(key, value, CancellationToken.None));

			// make sure it's there
			var item = await (cache.GetAsync(key, CancellationToken.None));
			Assert.That(item, Is.Not.Null, "item just added is not there");

			// remove it
			await (cache.RemoveAsync(key, CancellationToken.None));

			// make sure it's not there
			item = await (cache.GetAsync(key, CancellationToken.None));
			Assert.That(item, Is.Null, "item still exists in cache after remove");
		}

		[Test, Repeat(2)]
		public async Task TestDistributedRemoveAsync()
		{
			if (!SupportsDistributedCache)
			{
				Assert.Ignore("Test not supported by provider");
			}

			const string key = "keyTestRemove";
			const string value = "valueRemove";

			var cache = GetDefaultCache();
			var cache2 = (CacheBase) GetNewProvider().BuildCache(DefaultRegion, GetDefaultProperties());
			var cache3 = (CacheBase) GetNewProvider().BuildCache(DefaultRegion, GetDefaultProperties());

			await (cache.RemoveAsync(key, CancellationToken.None));
			await (Task.Delay(DistributedSynhronizationTime));
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Null, "item still exists in cache after remove");
			Assert.That(await (cache2.GetAsync(key, CancellationToken.None)), Is.Null, "item still exists in cache after remove");
			Assert.That(await (cache3.GetAsync(key, CancellationToken.None)), Is.Null, "item still exists in cache after remove");

			await (cache.PutAsync(key, value, CancellationToken.None));
			await (Task.Delay(DistributedSynhronizationTime));
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Not.Null, "item just added is not there");
			Assert.That(await (cache2.GetAsync(key, CancellationToken.None)), Is.Not.Null, "item just added is not there");
			Assert.That(await (cache3.GetAsync(key, CancellationToken.None)), Is.Not.Null, "item just added is not there");

			await (cache.RemoveAsync(key, CancellationToken.None));
			await (Task.Delay(DistributedSynhronizationTime));
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Null, "item still exists in cache after remove");
			Assert.That(await (cache2.GetAsync(key, CancellationToken.None)), Is.Null, "item still exists in cache after remove");
			Assert.That(await (cache3.GetAsync(key, CancellationToken.None)), Is.Null, "item still exists in cache after remove");

			await (cache.PutAsync(key, value, CancellationToken.None));
		}

		[Test]
		public async Task TestLockUnlockAsync()
		{
			if (!SupportsLocking)
				Assert.Ignore("Test not supported by provider");

			const string key = "keyTestLock";
			const string value = "valueLock";

			var cache = GetDefaultCache();

			// add the item
			await (cache.PutAsync(key, value, CancellationToken.None));

			await (cache.LockAsync(key, CancellationToken.None));
			Assert.ThrowsAsync<CacheException>(() => cache.LockAsync(key, CancellationToken.None));

			await (Task.Delay(cache.Timeout / Timestamper.OneMs));

			for (var i = 0; i < 2; i++)
			{
				var lockValue = await (cache.LockAsync(key, CancellationToken.None));
				await (cache.UnlockAsync(key, lockValue, CancellationToken.None));
			}
		}

		[Test]
		public async Task TestDistributedLockUnlockAsync()
		{
			if (!SupportsLocking || !SupportsDistributedCache)
			{
				Assert.Ignore("Test not supported by provider");
			}

			const string key = "keyTestLock";
			const string value = "valueLock";

			var cache = GetDefaultCache();
			var cache2 = (CacheBase) GetNewProvider().BuildCache(DefaultRegion, GetDefaultProperties());
			var cache3 = (CacheBase) GetNewProvider().BuildCache(DefaultRegion, GetDefaultProperties());

			// add the item
			await (cache.PutAsync(key, value, CancellationToken.None));
			await (Task.Delay(DistributedSynhronizationTime));

			var lockValue = await (cache.LockAsync(key, CancellationToken.None));
			Assert.ThrowsAsync<CacheException>(() => cache.LockAsync(key, CancellationToken.None), "The key should be locked");
			Assert.ThrowsAsync<CacheException>(() => cache2.LockAsync(key, CancellationToken.None), "The key should be locked");
			Assert.ThrowsAsync<CacheException>(() => cache3.LockAsync(key, CancellationToken.None), "The key should be locked");
			await (cache.UnlockAsync(key, lockValue, CancellationToken.None));

			lockValue = await (cache2.LockAsync(key, CancellationToken.None));
			Assert.ThrowsAsync<CacheException>(() => cache.LockAsync(key, CancellationToken.None), "The key should be locked");
			Assert.ThrowsAsync<CacheException>(() => cache2.LockAsync(key, CancellationToken.None), "The key should be locked");
			Assert.ThrowsAsync<CacheException>(() => cache3.LockAsync(key, CancellationToken.None), "The key should be locked");
			await (cache2.UnlockAsync(key, lockValue, CancellationToken.None));
			await (Task.Delay(DistributedSynhronizationTime));

			for (var i = 0; i < 2; i++)
			{
				lockValue = await (cache.LockAsync(key, CancellationToken.None));
				await (cache.UnlockAsync(key, lockValue, CancellationToken.None));

				lockValue = await (cache2.LockAsync(key, CancellationToken.None));
				await (cache2.UnlockAsync(key, lockValue, CancellationToken.None));

				lockValue = await (cache3.LockAsync(key, CancellationToken.None));
				await (cache3.UnlockAsync(key, lockValue, CancellationToken.None));
			}
		}

		[Test]
		public async Task TestConcurrentLockUnlockAsync()
		{
			if (!SupportsLocking)
				Assert.Ignore("Test not supported by provider");

			const string value = "value";
			const string key = "keyToLock";

			var cache = GetDefaultCache();

			await (cache.PutAsync(key, value, CancellationToken.None));
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.EqualTo(value), "Unable to retrieved cached object for key");

			// Simulate NHibernate ReadWriteCache behavior with multiple concurrent threads
			// Thread 1
			var lockValue = await (cache.LockAsync(key, CancellationToken.None));
			// Thread 2
			Assert.ThrowsAsync<CacheException>(() => cache.LockAsync(key, CancellationToken.None), "The key should be locked");
			// Thread 3
			Assert.ThrowsAsync<CacheException>(() => cache.LockAsync(key, CancellationToken.None), "The key should still be locked");

			// Thread 1
			await (cache.UnlockAsync(key, lockValue, CancellationToken.None));

			Assert.DoesNotThrowAsync(async () => lockValue = await (cache.LockAsync(key, CancellationToken.None)), "The key should be unlocked");
			await (cache.UnlockAsync(key, lockValue, CancellationToken.None));

			await (cache.RemoveAsync(key, CancellationToken.None));
		}

		[Test]
		public async Task TestClearAsync()
		{
			if (!SupportsClear)
				Assert.Ignore("Test not supported by provider");

			const string key = "keyTestClear";
			const string value = "valueClear";

			var cache = GetDefaultCache();

			// add the item
			await (cache.PutAsync(key, value, CancellationToken.None));

			// make sure it's there
			var item = await (cache.GetAsync(key, CancellationToken.None));
			Assert.That(item, Is.Not.Null, "couldn't find item in cache");

			// clear the cache
			await (cache.ClearAsync(CancellationToken.None));

			// make sure we don't get an item
			item = await (cache.GetAsync(key, CancellationToken.None));
			Assert.That(item, Is.Null, "item still exists in cache after clear");
		}

		[Test]
		public async Task TestDistributedClearAsync()
		{
			if (!SupportsClear || !SupportsDistributedCache)
			{
				Assert.Ignore("Test not supported by provider");
			}

			const string key = "keyTestClear";
			const string value = "valueClear";

			var cache = GetDefaultCache();
			var cache2 = (CacheBase) GetNewProvider().BuildCache(DefaultRegion, GetDefaultProperties());
			var cache3 = (CacheBase) GetNewProvider().BuildCache(DefaultRegion, GetDefaultProperties());

			// add the item
			await (cache.PutAsync(key, value, CancellationToken.None));
			await (Task.Delay(DistributedSynhronizationTime));

			// make sure it's there
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Not.Null, "couldn't find item in cache");
			Assert.That(await (cache2.GetAsync(key, CancellationToken.None)), Is.Not.Null, "couldn't find item in cache");
			Assert.That(await (cache3.GetAsync(key, CancellationToken.None)), Is.Not.Null, "couldn't find item in cache");

			// clear the cache
			await (cache.ClearAsync(CancellationToken.None));
			await (Task.Delay(DistributedSynhronizationTime));

			// make sure we don't get an item
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Null, "item still exists in cache after clear");
			Assert.That(await (cache2.GetAsync(key, CancellationToken.None)), Is.Null, "item still exists in cache after clear");
			Assert.That(await (cache3.GetAsync(key, CancellationToken.None)), Is.Null, "item still exists in cache after clear");
		}

		[Test]
		public void TestNullKeyPutAsync()
		{
			var cache = GetDefaultCache();
			Assert.ThrowsAsync<ArgumentNullException>(() => cache.PutAsync(null, null, CancellationToken.None));
		}

		[Test]
		public void TestNullValuePutAsync()
		{
			var cache = GetDefaultCache();
			Assert.ThrowsAsync<ArgumentNullException>(() => cache.PutAsync("keyTestNullValuePut", null, CancellationToken.None));
		}

		[Test]
		public async Task TestNullKeyGetAsync()
		{
			var cache = GetDefaultCache();
			await (cache.PutAsync("keyTestNullKeyGet", "value", CancellationToken.None));
			var item = await (cache.GetAsync(null, CancellationToken.None));
			Assert.IsNull(item);
		}

		[Test]
		public void TestNullKeyRemoveAsync()
		{
			var cache = GetDefaultCache();
			Assert.ThrowsAsync<ArgumentNullException>(() => cache.RemoveAsync(null, CancellationToken.None));
		}

		[Test]
		public async Task TestRegionsAsync()
		{
			const string key = "keyTestRegions";
			var props = GetDefaultProperties();
			var cache1 = DefaultProvider.BuildCache("TestRegions1", props);
			var cache2 = DefaultProvider.BuildCache("TestRegions2", props);
			const string s1 = "test1";
			const string s2 = "test2";
			await (cache1.PutAsync(key, s1, CancellationToken.None));
			await (cache2.PutAsync(key, s2, CancellationToken.None));
			var get1 = await (cache1.GetAsync(key, CancellationToken.None));
			var get2 = await (cache2.GetAsync(key, CancellationToken.None));
			Assert.That(get1, Is.EqualTo(s1), "Unexpected value in cache1");
			Assert.That(get2, Is.EqualTo(s2), "Unexpected value in cache2");
		}

		[Test]
		public async Task TestPutManyAsync()
		{
			var keys = new object[10];
			var values = new object[10];
			for (var i = 0; i < keys.Length; i++)
			{
				keys[i] = $"keyTestPut{i}";
				values[i] = $"valuePut{i}";
			}

			var cache = GetDefaultCache();
			// Due to async version, it may already be there.
			foreach (var key in keys)
				await (cache.RemoveAsync(key, CancellationToken.None));

			Assert.That(await (cache.GetManyAsync(keys, CancellationToken.None)), Is.EquivalentTo(new object[10]), "cache returned items we didn't add !?!");

			await (cache.PutManyAsync(keys, values, CancellationToken.None));
			var items = await (cache.GetManyAsync(keys, CancellationToken.None));

			for (var i = 0; i < items.Length; i++)
			{
				var item = items[i];
				Assert.That(item, Is.Not.Null, "unable to retrieve cached item");
				Assert.That(item, Is.EqualTo(values[i]), "didn't return the item we added");
			}
		}

		[Test]
		public async Task TestDistributedPutManyAsync()
		{
			if (!SupportsDistributedCache)
			{
				Assert.Ignore("Test not supported by provider");
			}

			var keys = new object[10];
			var values = new object[10];
			for (var i = 0; i < keys.Length; i++)
			{
				keys[i] = $"keyTestPut{i}";
				values[i] = $"valuePut{i}";
			}

			var cache = GetDefaultCache();
			var cache2 = (CacheBase) GetNewProvider().BuildCache(DefaultRegion, GetDefaultProperties());
			var cache3 = (CacheBase) GetNewProvider().BuildCache(DefaultRegion, GetDefaultProperties());
			// Due to async version, it may already be there.
			foreach (var key in keys)
				await (cache.RemoveAsync(key, CancellationToken.None));

			await (Task.Delay(DistributedSynhronizationTime));
			Assert.That(await (cache.GetManyAsync(keys, CancellationToken.None)), Is.EquivalentTo(new object[10]), "cache returned items we didn't add !?!");
			Assert.That(await (cache2.GetManyAsync(keys, CancellationToken.None)), Is.EquivalentTo(new object[10]), "cache returned items we didn't add !?!");
			Assert.That(await (cache3.GetManyAsync(keys, CancellationToken.None)), Is.EquivalentTo(new object[10]), "cache returned items we didn't add !?!");

			await (cache.PutManyAsync(keys, values, CancellationToken.None));
			await (Task.Delay(DistributedSynhronizationTime));

			AssertNotEmpty(await (cache.GetManyAsync(keys, CancellationToken.None)));
			AssertNotEmpty(await (cache2.GetManyAsync(keys, CancellationToken.None)));
			AssertNotEmpty(await (cache3.GetManyAsync(keys, CancellationToken.None)));

			void AssertNotEmpty(object[] items)
			{
				for (var i = 0; i < items.Length; i++)
				{
					var item = items[i];
					Assert.That(item, Is.Not.Null, "unable to retrieve cached item");
					Assert.That(item, Is.EqualTo(values[i]), "didn't return the item we added");
				}
			}
		}

		[Test]
		public async Task TestLockUnlockManyAsync()
		{
			if (!SupportsLocking)
				Assert.Ignore("Test not supported by provider");

			var keys = new object[10];
			var values = new object[10];
			for (var i = 0; i < keys.Length; i++)
			{
				keys[i] = $"keyTestLock{i}";
				values[i] = $"valueLock{i}";
			}

			var cache = GetDefaultCache();

			// add the item
			await (cache.PutManyAsync(keys, values, CancellationToken.None));
			await (cache.LockManyAsync(keys, CancellationToken.None));
			Assert.ThrowsAsync<CacheException>(() => cache.LockManyAsync(keys, CancellationToken.None), "all items should be locked");

			await (Task.Delay(cache.Timeout / Timestamper.OneMs));

			for (var i = 0; i < 2; i++)
			{
				Assert.DoesNotThrowAsync(async () =>
				{
					await (cache.UnlockManyAsync(keys, await (cache.LockManyAsync(keys, CancellationToken.None)), CancellationToken.None));
				}, "the items should be unlocked");
			}

			// Test partial locks by locking the first 5 keys and afterwards try to lock last 6 keys.
			var lockValue = await (cache.LockManyAsync(keys.Take(5).ToArray(), CancellationToken.None));

			Assert.ThrowsAsync<CacheException>(() => cache.LockManyAsync(keys.Skip(4).ToArray(), CancellationToken.None), "the fifth key should be locked");

			Assert.DoesNotThrowAsync(async () =>
			{
				await (cache.UnlockManyAsync(keys, await (cache.LockManyAsync(keys.Skip(5).ToArray(), CancellationToken.None)), CancellationToken.None));
			}, "the last 5 keys should not be locked.");

			// Unlock the first 5 keys
			await (cache.UnlockManyAsync(keys, lockValue, CancellationToken.None));

			Assert.DoesNotThrowAsync(async () =>
			{
				lockValue = await (cache.LockManyAsync(keys, CancellationToken.None));
				await (cache.UnlockManyAsync(keys, lockValue, CancellationToken.None));
			}, "the first 5 keys should not be locked.");
		}

		[Test]
		public async Task TestDistributedLockUnlockManyAsync()
		{
			if (!SupportsDistributedCache)
			{
				Assert.Ignore("Test not supported by provider");
			}

			var keys = new object[10];
			var values = new object[10];
			for (var i = 0; i < keys.Length; i++)
			{
				keys[i] = $"keyTestLock{i}";
				values[i] = $"valueLock{i}";
			}

			var cache = GetDefaultCache();
			var cache2 = (CacheBase) GetNewProvider().BuildCache(DefaultRegion, GetDefaultProperties());
			var cache3 = (CacheBase) GetNewProvider().BuildCache(DefaultRegion, GetDefaultProperties());

			// add the item
			await (cache.PutManyAsync(keys, values, CancellationToken.None));
			await (cache.LockManyAsync(keys, CancellationToken.None));

			Assert.ThrowsAsync<CacheException>(() => cache.LockManyAsync(keys, CancellationToken.None), "all items should be locked");
			Assert.ThrowsAsync<CacheException>(() => cache2.LockManyAsync(keys, CancellationToken.None), "all items should be locked");
			Assert.ThrowsAsync<CacheException>(() => cache3.LockManyAsync(keys, CancellationToken.None), "all items should be locked");

			await (Task.Delay(cache.Timeout / Timestamper.OneMs));

			for (var i = 0; i < 2; i++)
			{
				Assert.DoesNotThrowAsync(async () =>
				{
					await (cache.UnlockManyAsync(keys, await (cache.LockManyAsync(keys, CancellationToken.None)), CancellationToken.None));
					await (cache2.UnlockManyAsync(keys, await (cache2.LockManyAsync(keys, CancellationToken.None)), CancellationToken.None));
					await (cache3.UnlockManyAsync(keys, await (cache3.LockManyAsync(keys, CancellationToken.None)), CancellationToken.None));
				}, "the items should be unlocked");
			}

			// Test partial locks by locking the first 5 keys and afterwards try to lock last 6 keys.
			var lockValue = await (cache.LockManyAsync(keys.Take(5).ToArray(), CancellationToken.None));

			Assert.ThrowsAsync<CacheException>(() => cache.LockManyAsync(keys.Skip(4).ToArray(), CancellationToken.None), "the fifth key should be locked");
			Assert.ThrowsAsync<CacheException>(() => cache2.LockManyAsync(keys.Skip(4).ToArray(), CancellationToken.None), "the fifth key should be locked");
			Assert.ThrowsAsync<CacheException>(() => cache3.LockManyAsync(keys.Skip(4).ToArray(), CancellationToken.None), "the fifth key should be locked");

			Assert.DoesNotThrowAsync(async () =>
			{
				await (cache.UnlockManyAsync(keys, await (cache.LockManyAsync(keys.Skip(5).ToArray(), CancellationToken.None)), CancellationToken.None));
				await (cache2.UnlockManyAsync(keys, await (cache2.LockManyAsync(keys.Skip(5).ToArray(), CancellationToken.None)), CancellationToken.None));
				await (cache3.UnlockManyAsync(keys, await (cache3.LockManyAsync(keys.Skip(5).ToArray(), CancellationToken.None)), CancellationToken.None));
			}, "the last 5 keys should not be locked.");

			// Unlock the first 5 keys
			await (cache.UnlockManyAsync(keys, lockValue, CancellationToken.None));

			Assert.DoesNotThrowAsync(async () =>
			{
				await (cache.UnlockManyAsync(keys, await (cache.LockManyAsync(keys, CancellationToken.None)), CancellationToken.None));
				await (cache2.UnlockManyAsync(keys, await (cache2.LockManyAsync(keys, CancellationToken.None)), CancellationToken.None));
				await (cache3.UnlockManyAsync(keys, await (cache3.LockManyAsync(keys, CancellationToken.None)), CancellationToken.None));
			}, "the first 5 keys should not be locked.");
		}

		[Test]
		public void TestNullKeyPutManyAsync()
		{
			var cache = GetDefaultCache();
			Assert.ThrowsAsync<ArgumentNullException>(() => cache.PutManyAsync(null, null, CancellationToken.None));
		}

		[Test]
		public void TestNullValuePutManyAsync()
		{
			var cache = GetDefaultCache();
			Assert.ThrowsAsync<ArgumentNullException>(() => cache.PutManyAsync(new object[] { "keyTestNullValuePut" }, null, CancellationToken.None));
		}

		[Test]
		public async Task TestNullKeyGetManyAsync()
		{
			var cache = GetDefaultCache();
			await (cache.PutAsync("keyTestNullKeyGet", "value", CancellationToken.None));
			Assert.ThrowsAsync<ArgumentNullException>(() => cache.GetManyAsync(null, CancellationToken.None));
		}

		[Test]
		public async Task TestNonEqualObjectsWithEqualHashCodeAndToStringAsync()
		{
			if (!SupportsDistinguishingKeysWithSameStringRepresentationAndHashcode)
				Assert.Ignore("Test not supported by provider");

			var obj1 = new SomeObject();
			var obj2 = new SomeObject();

			obj1.Id = 1;
			obj2.Id = 2;

			var cache = GetDefaultCache();

			Assert.That(await (cache.GetAsync(obj2, CancellationToken.None)), Is.Null, "Unexectedly found a cache entry for key obj2");
			await (cache.PutAsync(obj1, obj1, CancellationToken.None));
			Assert.That(await (cache.GetAsync(obj1, CancellationToken.None)), Is.EqualTo(obj1), "Unable to retrieved cached object for key obj1");
			Assert.That(await (cache.GetAsync(obj2, CancellationToken.None)), Is.Null, "Unexectedly found a cache entry for key obj2 after obj1 put");
		}

		[Test]
		public async Task TestObjectExpirationAsync([ValueSource(nameof(ExpirationSettingNames))] string expirationSetting)
		{
			if (!SupportsDefaultExpiration)
				Assert.Ignore("Provider does not support default expiration settings");

			const int expirySeconds = 3;
			const string key = "keyTestObjectExpiration";
			var obj = new SomeObject { Id = 2 };

			var cache = GetCacheForExpiration("TestObjectExpiration", expirationSetting, expirySeconds);

			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Null, "Unexpected entry for key");
			await (cache.PutAsync(key, obj, CancellationToken.None));
			// Wait up to 1 sec before expiration
			await (Task.Delay(TimeSpan.FromSeconds(expirySeconds - 1)));
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Not.Null, "Missing entry for key");

			// Wait expiration
			await (Task.Delay(TimeSpan.FromSeconds(2)));

			// Check it expired
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Null, "Unexpected entry for key after expiration");
		}

		[Test]
		public async Task TestObjectExpirationAfterUpdateAsync([ValueSource(nameof(ExpirationSettingNames))] string expirationSetting)
		{
			if (!SupportsDefaultExpiration)
				Assert.Ignore("Provider does not support default expiration settings");

			const int expirySeconds = 3;
			const string key = "keyTestObjectExpirationAfterUpdate";
			var obj = new SomeObject { Id = 2 };

			var cache = GetCacheForExpiration("TestObjectExpirationAfterUpdate", expirationSetting, expirySeconds);

			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Null, "Unexpected entry for key");
			await (cache.PutAsync(key, obj, CancellationToken.None));
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Not.Null, "Missing entry for key");

			// This forces an object update
			await (cache.PutAsync(key, obj, CancellationToken.None));
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Not.Null, "Missing entry for key after update");

			// Wait
			await (Task.Delay(TimeSpan.FromSeconds(expirySeconds + 2)));

			// Check it expired
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Null, "Unexpected entry for key after expiration");
		}

		[Test]
		public async Task TestSlidingExpirationAsync()
		{
			if (!SupportsSlidingExpiration)
				Assert.Ignore("Provider does not support sliding expiration settings");

			const int expirySeconds = 3;
			const string key = "keyTestSlidingExpiration";
			var obj = new SomeObject { Id = 2 };

			var props = GetPropertiesForExpiration(Cfg.Environment.CacheDefaultExpiration, expirySeconds.ToString());
			props["cache.use_sliding_expiration"] = "true";
			var cache = DefaultProvider.BuildCache("TestObjectExpiration", props);

			await (cache.PutAsync(key, obj, CancellationToken.None));
			// Wait up to 1 sec before expiration
			await (Task.Delay(TimeSpan.FromSeconds(expirySeconds - 1)));
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Not.Null, "Missing entry for key");

			// Wait up to 1 sec before expiration again
			await (Task.Delay(TimeSpan.FromSeconds(expirySeconds - 1)));
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Not.Null, "Missing entry for key after get and wait less than expiration");

			// Wait expiration
			await (Task.Delay(TimeSpan.FromSeconds(expirySeconds + 1)));

			// Check it expired
			Assert.That(await (cache.GetAsync(key, CancellationToken.None)), Is.Null, "Unexpected entry for key after expiration");
		}

		// NHCH-43
		[Test]
		public async Task TestUnicodeAsync()
		{
			var keyValues = new Dictionary<string, string>
			{
				{"길동", "valuePut1"},
				{"최고", "valuePut2"},
				{"新闻", "valuePut3"},
				{"地图", "valuePut4"},
				{"ます", "valuePut5"},
				{"プル", "valuePut6"}
			};
			var cache = GetDefaultCache();

			// Troubles may specifically arise with long keys, where a hashing algorithm may be used.
			var longKeyPrefix = new string('_', 1000);
			var longKeyValueSuffix = "Long";
			foreach (var kv in keyValues)
			{
				await (cache.PutAsync(kv.Key, kv.Value, CancellationToken.None));
				await (cache.PutAsync(longKeyPrefix + kv.Key, kv.Value + longKeyValueSuffix, CancellationToken.None));
			}

			foreach (var kv in keyValues)
			{
				var item = await (cache.GetAsync(kv.Key, CancellationToken.None));
				Assert.That(item, Is.EqualTo(kv.Value), $"Didn't return the item we added for key {kv.Key}");
				item = await (cache.GetAsync(longKeyPrefix + kv.Key, CancellationToken.None));
				Assert.That(item, Is.EqualTo(kv.Value + longKeyValueSuffix), $"Didn't return the item we added for long key {kv.Key}");
			}
		}

		[Test]
		public async Task TestRepeatedPutAsync()
		{
			const string key = "keyTestPut";
			const string value = "valuePut";
			const string value2 = "valuePut2";
			var cache = GetDefaultCache();
			for (var i = 0; i < 100; i++)
			{
				await (cache.PutAsync(key, value, CancellationToken.None));
				await (cache.PutAsync(key, value2, CancellationToken.None));
				var item = await (cache.GetAsync(key, CancellationToken.None));
				Assert.That(item, Is.Not.Null, "Unable to retrieve cached item");
				Assert.That(item, Is.EqualTo(value2), "didn't return the item we added");
			}
		}

		[Test]
		public async Task TestRepeatedRemovePutAsync()
		{
			const string key = "keyTestPut";
			const string value = "valuePut";
			var cache = GetDefaultCache();
			for (var i = 0; i < 100; i++)
			{
				await (cache.RemoveAsync(key, CancellationToken.None));
				await (cache.PutAsync(key, value, CancellationToken.None));
				var item = await (cache.GetAsync(key, CancellationToken.None));
				Assert.That(item, Is.Not.Null, "Unable to retrieve cached item");
				Assert.That(item, Is.EqualTo(value), "didn't return the item we added");
			}
		}

		[Test]
		public async Task TestRepeatedPutRemoveAsync()
		{
			const string key = "keyTestPut";
			const string value = "valuePut";
			var cache = GetDefaultCache();
			for (var i = 0; i < 100; i++)
			{
				await (cache.PutAsync(key, value, CancellationToken.None));
				await (cache.RemoveAsync(key, CancellationToken.None));
				var item = await (cache.GetAsync(key, CancellationToken.None));
				Assert.That(item, Is.Null, "Item still exists in cache after remove");
			}
		}

		[Test]
		public async Task TestRepeatedClearPutAsync()
		{
			if (!SupportsClear)
			{
				Assert.Ignore("Test not supported by provider");
			}

			const string key = "keyTestPut";
			const string value = "valuePut";
			var cache = GetDefaultCache();
			for (var i = 0; i < 100; i++)
			{
				await (cache.ClearAsync(CancellationToken.None));
				await (cache.PutAsync(key, value, CancellationToken.None));
				var item = await (cache.GetAsync(key, CancellationToken.None));
				Assert.That(item, Is.Not.Null, "Unable to retrieve cached item");
				Assert.That(item, Is.EqualTo(value), "didn't return the item we added");

			}
		}

		[Test]
		public async Task TestRepeatedPutClearAsync()
		{
			if (!SupportsClear)
			{
				Assert.Ignore("Test not supported by provider");
			}

			const string key = "keyTestPut";
			const string value = "valuePut";
			var cache = GetDefaultCache();
			for (var i = 0; i < 100; i++)
			{
				await (cache.PutAsync(key, value, CancellationToken.None));
				await (cache.ClearAsync(CancellationToken.None));
				var item = await (cache.GetAsync(key, CancellationToken.None));
				Assert.That(item, Is.Null, "Item still exists in cache after clear");
			}
		}
	}
}
