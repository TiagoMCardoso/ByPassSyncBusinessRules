using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace TMC.ByPassSyncBusinessRules
{
    public class Contact : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                tracingService.Trace($"Contact Plugin Start");

                try
                {
                    // Plug-in business logic goes here.
                    Entity task = new Entity("task");
                    task["regardingobjectid"] = entity.ToEntityReference();
                    task["subject"] = "Task associated to the current contact";

                    var createRequest = new CreateRequest
                    {
                        Target = task
                    };

                    service.Execute(createRequest);
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException($"An error occurred in Contact Plugin {ex.ToString()}");
                }

                catch (Exception ex)
                {
                    tracingService.Trace($"Contact Plugin Error:  {ex.ToString()}");
                    throw;
                }
                finally
                {
                    tracingService.Trace($"Contact Plugin End");
                }
            }
        }
    }
}