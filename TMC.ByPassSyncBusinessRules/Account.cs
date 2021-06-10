using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace TMC.ByPassSyncBusinessRules
{
    public class Account : IPlugin
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

                tracingService.Trace($"Account Plugin Start");

                try
                {
                    // Plug-in business logic goes here.  
                    Entity contact = new Entity("contact");
                    contact["parentcustomerid"] = entity.ToEntityReference();
                    contact["firstname"] = "Test";
                    contact["lastname"] = DateTime.Now.ToString();

                    var createRequest = new CreateRequest
                    {
                        Target = contact
                    };
                    
                    // Bypass set to TRUE (Contact Plugin will be skipped)
                    createRequest.Parameters.Add("BypassCustomPluginExecution", true);

                    service.Execute(createRequest);
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException($"An error occurred in Account Plugin {ex.ToString()}");
                }

                catch (Exception ex)
                {
                    tracingService.Trace($"Account Plugin Error: {ex.ToString()}");
                    throw;
                }
                finally
                {
                    tracingService.Trace($"Account Plugin End");
                }
            }
        }
    }
}
