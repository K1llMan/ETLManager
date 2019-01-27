using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

using Newtonsoft.Json.Linq;

using ETLCommon;

using ETLService.Manager;

namespace ETLService.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class RootController : Controller
    {
        // GET api/methods
        [HttpGet("methods")]
        public object GetMethods()
        {
            JObject apiList = new JObject();
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes().Where(t => t.Name.IsMatch("Controller")))
            {
                JArray controllerMethods = new JArray();

                RouteAttribute route = (RouteAttribute)type.GetCustomAttributes(typeof(RouteAttribute)).FirstOrDefault();
                string controllerName = type.Name.Replace("Controller", string.Empty);
                string mainRoute = route.Template.Replace("[controller]", controllerName.ToLower());

                foreach (MethodInfo method in type.GetMethods())
                    foreach (HttpMethodAttribute attr in method.GetCustomAttributes(typeof(HttpMethodAttribute)))
                    {
                        JObject apiMethod = new JObject {
                            { "route", $"{string.Join(", ", attr.HttpMethods)} {string.Join("/", new string[] { mainRoute, attr.Template }.Where(s => !string.IsNullOrEmpty(s)))}" }
                        };

                        AuthorizeAttribute authAttr = (AuthorizeAttribute)method.GetCustomAttributes(typeof(AuthorizeAttribute)).FirstOrDefault();
                        if (authAttr != null)
                            apiMethod.Add("auth", new JObject{
                                { "roles", authAttr.Roles },
                                { "policy", authAttr.Policy }
                            });

                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters.Length > 0)
                        {
                            JArray methodParams = new JArray();
                            foreach (ParameterInfo parameter in method.GetParameters())
                                methodParams.Add(new JObject {
                                    { "name", parameter.Name },
                                    { "type", parameter.ParameterType.Name },
                                    { "default", parameter.RawDefaultValue.ToString() }
                                });
                            apiMethod.Add("params", methodParams);
                        }
                        controllerMethods.Add(apiMethod);
                    }

                apiList.Add(controllerName, controllerMethods);
            }

            return apiList;
        }

        // GET api/modules
        /// <summary>
        /// Возвращает доступные для отображения модули
        /// </summary>
        [HttpGet("modules")]
        public object GetModules()
        {
            if (User.IsInRole("Admin"))
                return new Dictionary<string, Dictionary<string, string>> {
                    { string.Empty, new Dictionary<string, string> {
                            { "displayName", "Admin scenarios" },
                            { "script", "Scenarios" }
                        } 
                    }
                };

            return new Dictionary<string, Dictionary<string, string>> {
                { string.Empty, new Dictionary<string, string> {
                        { "displayName", "Scenarios" },
                        { "script", "Scenarios" }
                    }
                },
                { "#history", new Dictionary<string, string> {
                        { "displayName", "History" },
                        { "script", "History" }
                    }
                },
                { "#test", new Dictionary<string, string> {
                        { "displayName", "Test" },
                        { "script", "Test" }
                    }
                }
            };
        }

        // GET api/info
        [HttpGet("info")]
        public object GetVersion()
        {
            return new Dictionary<string, object> {
                { "version", Program.Manager.Context.Version }
            };
        }

        // GET api/updates
        [HttpGet("updates")]
        public object GetUpdates()
        {
            return WebAPI.Success(Program.Manager.UpdateManager.Updates);
        }

        // PUT api/updates
        [HttpPut("updates")]
        public object UpdateRegistry([FromBody]string[] updates)
        {
            int count = Program.Manager.UpdateManager.Updates.Count;
            return WebAPI.Success($"Применено {Program.Manager.ApplyUpdates(updates)} из {count}");
        }

    }
}