using System.Threading.Tasks;
using EvidentTestDashboard.Library.Contracts;
using EvidentTestDashboard.Library.Factories;
using EvidentTestDashboard.Library.Services;
using System.Linq;
using System.Text.RegularExpressions;
using EvidentTestDashboard.Library.Entities;
using System;

namespace EvidentTestDashboard.Library.Jobs
{
    public class BuildInformationJob
    {
        private readonly ITestDashboardUOW _uow;
        private readonly ITeamCityService _teamCityService;

        public BuildInformationJob(ITestDashboardUOW uow, ITeamCityService teamCityService)
        {
            _uow = uow;
            _teamCityService = teamCityService;
        }

        public async Task CollectBuildDataAsync(DateTime sinceDate)
        {
            var buildTypeNames = _uow.BuildTypes.GetAll().Where(bt => bt.Environment.Dashboard.DashboardName == Settings.DefaultDashboard).Select(bt => bt.BuildTypeName).Distinct();
            var latestBuildDTOs = await _teamCityService.GetBuildsAsync(buildTypeNames, sinceDate);

            var latestBuilds = latestBuildDTOs
                .Select(bDto => BuildFactory.Instance.Create(bDto))
                .ToDictionary(b => b.TeamCityBuildId, b => b);

            foreach (var buildDto in latestBuildDTOs)
            {
                var buildTypes =
                    _uow.BuildTypes.GetAll().Where(bt => bt.BuildTypeName == buildDto.BuildTypeId).ToList();

                if (buildTypes != null && buildTypes.Any())
                {
                    var build = latestBuilds[buildDto.Id];

                    // Check if build isn't already saved in the database
                    if (!_uow.Builds.GetAll().Any(b => b.TeamCityBuildId == buildDto.Id))
                    {
                        BuildType buildType = null;
                        if (buildTypes.Count > 1)
                        {
                            // we have multiple build types of the same name..
                            // prob. because we have a parameter we need to check..
                            buildType = buildTypes.FirstOrDefault(bt => {
                                return buildDto.Properties.Property.Any(p => p.Name == bt.RequiredParamName && p.Value == bt.RequiredParamValue);
                            });
                        }
                        else
                        {
                            buildType = buildTypes.First();
                        }
                        if(buildType == null)
                        {
                            continue;
                        }
                        buildType.Builds.Add(build);

                        var testOccurrencesForBuildDTO =
                            await _teamCityService.GetTestOccurrencesForBuildAsync(build.TeamCityBuildId);

                        var testOccurrencesForBuild =
                            testOccurrencesForBuildDTO.Select(t => TestOccurrenceFactory.Instance.Create(t)).ToList();
                        testOccurrencesForBuild.ForEach(t => build.TestOccurrences.Add(t));

                        // Add dashboard filter..
                        var labels = _uow.Labels.GetAll().Where(l => l.Dashboard.DashboardName == Settings.DefaultDashboard).ToList();
                        foreach (var test in testOccurrencesForBuild)
                        {
                            var label = labels.SingleOrDefault(l => l.Regex != null
                                                && new Regex(l.Regex, RegexOptions.IgnoreCase).IsMatch(test.Name))
                                ?? labels.SingleOrDefault(l => l.Regex == null);
                            label?.TestOccurrences.Add(test);
                        }

                        _uow.Commit();
                    }
                }
            }
        }

    }
}