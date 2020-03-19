using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Recipes.Models;
using Xunit;

namespace OrchardCore.Tests.Deployment
{
    public class DeploymentSourceOrderTests
    {
        private static readonly IList<string> _executedDeploymentSteps = new List<string>();

        [Fact]
        public async Task ExecuteDeploymentPlan_ShouldRespect_DeployementSourceOrder()
        {
            var deploymentSources = new IDeploymentSource[] { new DeploymentSource1(), new DeploymentSource2(), new DeploymentSource3() };
            var deploymentManager = new DeploymentManager(deploymentSources, null, null);
            var recipeDescriptor = new RecipeDescriptor();

            _executedDeploymentSteps.Clear();

            using (var fileBuilder = new TemporaryFileBuilder())
            {
                var deploymentPlanResult = new DeploymentPlanResult(fileBuilder, recipeDescriptor);
                var deploymentPlan = new DeploymentPlan();

                deploymentPlan.DeploymentSteps.AddRange(new DeploymentStep[] { new DeploymentStep1(), new DeploymentStep2() });
                await deploymentManager.ExecuteDeploymentPlanAsync(deploymentPlan, deploymentPlanResult);
            }

            Assert.Equal("Deployment Step 2", _executedDeploymentSteps[0]);
            Assert.Equal("Deployment Step 1", _executedDeploymentSteps[1]);
            Assert.Equal("Deployment Step 3", _executedDeploymentSteps[2]);
        }

        [Fact]
        public async Task ExecuteDeploymentPlan_ExecuteDeployementSource_BeforeOrderedDeployementSource_WithZeroOrder()
        {
            var deploymentSources = new IDeploymentSource[] { new DeploymentSource1(), new DeploymentSource4() };
            var deploymentManager = new DeploymentManager(deploymentSources, null, null);
            var recipeDescriptor = new RecipeDescriptor();

            _executedDeploymentSteps.Clear();

            using (var fileBuilder = new TemporaryFileBuilder())
            {
                var deploymentPlanResult = new DeploymentPlanResult(fileBuilder, recipeDescriptor);
                var deploymentPlan = new DeploymentPlan();

                deploymentPlan.DeploymentSteps.AddRange(new DeploymentStep[] { new DeploymentStep1(), new DeploymentStep2() });
                await deploymentManager.ExecuteDeploymentPlanAsync(deploymentPlan, deploymentPlanResult);
            }

            Assert.Equal("Deployment Step 1", _executedDeploymentSteps[0]);
            Assert.Equal("Deployment Step 4", _executedDeploymentSteps[1]);
        }

        public class DeploymentStep1 : DeploymentStep
        {
            public DeploymentStep1()
            {
                Name = "Deployment Step 1";
            }
        }

        public class DeploymentStep2 : DeploymentStep
        {
            public DeploymentStep2()
            {
                Name = "Deployment Step 2";
            }
        }

        public class DeploymentSource1 : IDeploymentSource
        {
            public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
            {
                var deploymentStep = step as DeploymentStep1;

                if (deploymentStep == null)
                {
                    return;
                }

                result.Steps.Add(new JObject(new JProperty("name", "Deployment Step 1")));
                _executedDeploymentSteps.Add("Deployment Step 1");

                await Task.CompletedTask;
            }
        }

        public class DeploymentSource2 : IOrderedDeploymentSource
        {
            public int Order => -10;

            public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
            {
                var deploymentStep = step as DeploymentStep2;

                if (deploymentStep == null)
                {
                    return;
                }

                result.Steps.Add(new JObject(new JProperty("name", "Deployment Step 2")));
                _executedDeploymentSteps.Add("Deployment Step 2");

                await Task.CompletedTask;
            }
        }

        public class DeploymentSource3 : IOrderedDeploymentSource
        {
            public int Order => 10;

            public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
            {
                var deploymentStep = step as DeploymentStep2;

                if (deploymentStep == null)
                {
                    return;
                }

                result.Steps.Add(new JObject(new JProperty("name", "Deployment Step 3")));
                _executedDeploymentSteps.Add("Deployment Step 3");

                await Task.CompletedTask;
            }
        }

        public class DeploymentSource4 : IOrderedDeploymentSource
        {
            public int Order => 0;

            public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
            {
                var deploymentStep = step as DeploymentStep2;

                if (deploymentStep == null)
                {
                    return;
                }

                result.Steps.Add(new JObject(new JProperty("name", "Deployment Step 4")));
                _executedDeploymentSteps.Add("Deployment Step 4");

                await Task.CompletedTask;
            }
        }
    }
}
