﻿using NUnit.Framework;

namespace NHibernate.Caches.StackExchangeRedis.Tests
{
	[TestFixture]
	public class RedisCacheFastStrategyFixture : RedisCacheFixture<FastRegionStrategy>
	{
		protected override bool SupportsClear => false;
	}
}
