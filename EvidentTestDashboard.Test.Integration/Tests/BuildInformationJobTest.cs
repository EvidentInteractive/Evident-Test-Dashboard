﻿using EvidentTestDashboard.Library.Repositories;
using EvidentTestDashboard.Library.Services;
using EvidentTestDashboard.Web.Jobs;
using Xunit;

namespace EvidentTestDashboard.Test.Integration.Tests
{
    public class BuildInformationJobTest
    {
        [Fact]
        public async void ShouldAddLatestBuildToDb()
        {
            var sut = new BuildInformationJob(new TestDashboardUOW(), new TeamCityService());
            await sut.CollectBuildDataAsync();

            string bla = "";
        }
    }
}