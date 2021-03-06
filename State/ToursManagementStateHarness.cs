using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Fathym;
using LCU.Presentation.State.ReqRes;
using LCU.StateAPI.Utilities;
using LCU.StateAPI;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Collections.Generic;
using LCU.Personas.Client.Enterprises;
using LCU.Personas.Client.DevOps;
using LCU.Personas.Enterprises;
using LCU.Personas.Client.Applications;
using Fathym.API;
using LCU.Personas.Client.Identity;

namespace LCU.State.API.NapkinIDE.NapkinIDE.ToursManagement.State
{
    public class ToursManagementStateHarness : LCUStateHarness<ToursManagementState>
    {
        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public ToursManagementStateHarness(ToursManagementState state)
            : base(state ?? new ToursManagementState())
        { }
        #endregion

        #region API Methods
        public virtual void LoadGuidedTours()
        {
            State.Tours = new List<GuidedTour>();

            State.Tours.Add(createDemoTour("demo-tour"));

            State.Tours.Add(createLimitedTrialTour("limited-trial-tour"));

            State.Tours.Add(createDataApplicationsTour("data-applications-tour"));

            State.Tours.Add(createDataFlowManagementTour("data-flow-management-tour"));

            State.Tours.Add(createDataFlowToolTour("data-flow-tool-tour"));

            State.Tours.Add(createIoTDeveloperJourneyTour("iot-developer-journey-tour"));
        }

        public virtual void RecordStep(string tourLookup, string currentStep, bool isComplete)
        {
            if (State.StepRecords.IsNullOrEmpty())
                State.StepRecords = new Dictionary<string, GuidedTourStepRecord>();

            if (!State.StepRecords.ContainsKey(tourLookup))
                State.StepRecords[tourLookup] = new GuidedTourStepRecord() { StepHistory = new List<string>() };

            State.StepRecords[tourLookup].CurrentStep = currentStep;

            State.StepRecords[tourLookup].StepHistory.Add(State.StepRecords[tourLookup].CurrentStep);

            if (isComplete)
            {
                if (State.CompletedTourLookups.IsNullOrEmpty())
                    State.CompletedTourLookups = new Dictionary<string, string>();

                if (State.StepRecordHistory.IsNullOrEmpty())
                    State.StepRecordHistory = new Dictionary<string, List<GuidedTourStepRecord>>();

                if (!State.StepRecordHistory.ContainsKey(tourLookup))
                    State.StepRecordHistory[tourLookup] = new List<GuidedTourStepRecord>();

                State.CompletedTourLookups[tourLookup] = currentStep;

                State.StepRecordHistory[tourLookup].Add(State.StepRecords[tourLookup]);

                State.StepRecords.Remove(tourLookup);
            }
        }

        public virtual async Task Reset(IdentityManagerClient idMgr, string entApiKey, string username)
        {
            State = new ToursManagementState();

            await RefreshTours(idMgr, entApiKey, username);
        }

        public virtual async Task RefreshTours(IdentityManagerClient idMgr, string entApiKey, string username)
        {
            LoadGuidedTours();

            await SetToursEnabled(idMgr, entApiKey, username);

            if (State.CurrentTour == null && !State.ToursEnabled)
                State.CurrentTour = State.Tours.FirstOrDefault(tour => tour.Lookup == "limited-trial-tour");
        }

        public virtual async Task SetActiveTour(string entApiKey, string lookup)
        {
            State.CurrentTour = State.Tours.FirstOrDefault(tour => tour.Lookup == lookup);
        }

        public virtual async Task SetToursEnabled(IdentityManagerClient idMgr, string entApiKey, string username)
        {
            var authResp = await idMgr.HasAccess(entApiKey, username, new List<string>() { "LCU.NapkinIDE.AllAccess" });

            State.ToursEnabled = !authResp.Status;
        }
        #endregion

        #region Helpers
        protected virtual GuidedTour createCopyForNewTour(string lookup)
        {
            return new GuidedTour()
            {
                ID = new Guid("00000000-0000-0000-0000-0000000000**"), // Change and ensure Guid is unique to all other tours, by replacing ** with next value (01, 02, 03...11, 12...)
                Lookup = lookup,
                UseOrb = false,
                Steps = new List<GuidedTourStep>()
                {
                    new GuidedTourStep()
                    {
                        Title = "LCU-Guided-Tour",
                        Subtitle = "Guided Tour",
                        Lookup = "welcome",
                        Content = "Welcome to the LCU-Guided-Tour library! This library provides the functionality to do your own guided tour of an application. <br/><br/> Click the <b>Next</b> button to get started with an example Tour!"
                    },
                    new GuidedTourStep()
                    {
                        Title = "Title",
                        Subtitle = "Guided Tour",
                        Selector = "#guidedTourHeader",
                        Orientation = OrientationTypes.Bottom,
                        Lookup = "some-content",
                        Content = "This be some content"
                    },
                }
            };
        }

        protected virtual GuidedTour createDemoTour(string lookup)
        {
            return new GuidedTour()
            {
                ID = new Guid("00000000-0000-0000-0000-000000000001"),
                Lookup = lookup,
                UseOrb = false,
                Steps = new List<GuidedTourStep>()
                {
                    new GuidedTourStep()
                    {
                        Title = "LCU-Guided-Tour",
                        Subtitle = "Guided Tour",
                        Lookup = "welcome",
                        Content = "Welcome to the LCU-Guided-Tour library! This library provides the functionality to do your own guided tour of an application. <br/><br/> Click the <b>Next</b> button to get started with an example Tour!"
                    },
                    new GuidedTourStep()
                    {
                        Title = "Title",
                        Subtitle = "Guided Tour",
                        Selector = "#guidedTourHeader",
                        Orientation = OrientationTypes.Bottom,
                        Lookup = "header",
                        Content = "With the LCU-Guided-Tour, you can select anything that is on the screen that has a valid CSS selector.  For example, you can select this title, which as an id of <b>#guidedTourHeader</b>. <br/><br/> Valid selectors are as follows: <ul><li>.class</li><li>#id</li><li>element</li></ul>"
                    },
                    new GuidedTourStep()
                    {
                        Title = "First Paragraph",
                        Subtitle = "Guided Tour",
                        Selector = "p",
                        Orientation = OrientationTypes.BottomRight,
                        Lookup = "first",
                        Content = "Here, we are selecting the first paragraph element on the screen with <b>p</b>."
                    },
                    new GuidedTourStep()
                    {
                        Title = "Second Paragraph",
                        Subtitle = "Guided Tour",
                        Selector = "#p2",
                        Orientation = OrientationTypes.Top,
                        Lookup = "second",
                        Content = "Now we are selecting the second paragraph, that has an id of <b>#p2</b>, in which we are targeting."
                    },
                    new GuidedTourStep()
                    {
                        Title = "Complex Selectors",
                        Subtitle = "Guided Tour",
                        Selector = ".section:nth-of-type(2) .mat-radio-button:nth-child(3)",
                        Orientation = OrientationTypes.Right,
                        Lookup = "specific",
                        Content = "You can even target more specific, complex elements, by using various built-in CSS selectors. In this case, we are targeting the third radio item in the second section with the selector of: <br/> <b>.section:nth-of-type(2) .mat-radio-button:nth-child(3)</b>"
                    },
                    new GuidedTourStep()
                    {
                        Title = "Modifiers",
                        Subtitle = "Guided Tour",
                        Selector = "#formBox",
                        Orientation = OrientationTypes.Right,
                        Lookup = "customize",
                        Content = "As for the bot, you can modify certain properties of it in order to customize it to your needs. Here we can change the position it lives on the screen, the container it should position itself in, as well as the amount of padding we would like to have between the bot and the container."
                    },
                    new GuidedTourStep()
                    {
                        Title = "Bounding Container",
                        Subtitle = "Guided Tour",
                        Selector = "#boundingBox",
                        Orientation = OrientationTypes.Left,
                        Lookup = "inside",
                        Content = "As an example, you can set the Bot to be positioned inside this box by setting the container to the <b>#boundingBox</b> selector."
                    },
                    new GuidedTourStep()
                    {
                        Title = "Assigning Actions",
                        Subtitle = "Guided Tour",
                        Selector = ".mat-tab-label:nth-of-type(2)",
                        Orientation = OrientationTypes.BottomLeft,
                        Lookup = "assign",
                        Content = "You can assign each step an action as well, in case you want to run logic before or after a step is displayed.  Click <b>Next</b> to see this in action!"
                    },
                    new GuidedTourStep()
                    {
                        Title = "Tab Movement",
                        Subtitle = "Guided Tour",
                        Selector = "#boxLogoForm",
                        Orientation = OrientationTypes.BottomLeft,
                        ActionDelay = 500,
                        Lookup = "see",
                        Content = "As you can see, this tab was selected so that the Tour could continue after the DOM has rendered a different view.  You can also use the <b>actionDelay</b> property to specify a time delay before showing the next step, in order to properly render the next view."
                    }
                }
            };
        }

        protected virtual GuidedTour createLimitedTrialTour(string lookup)
        {
            return new GuidedTour()
            {
                ID = new Guid("00000000-0000-0000-0000-000000000002"),
                IsFirstTimeViewing = true,
                Lookup = lookup,
                UseOrb = false,
                Steps = new List<GuidedTourStep>()
                {
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000020"),
                        Title = "Welcome",
                        Subtitle = "Limited Trial Tour",
                        Lookup = "welcome",
                        Content = "Welcome to the live demo of the Fathym Low-Code Framework. I’m <b>Thinky</b>! I’ll guide you through a few tours to show you some of Fathym’s low-code tools, such as the <b>Data Flow Manager</b> and <b>Data Applications</b>, and I’ll explain how to interact and dig deeper with the tools so you can customize them for your needs."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000021"),
                        Title = "Welcome Page Resources",
                        Subtitle = "Limited Trial Tour",
                        Selector = "lcu-limited-trial-welcome-element > .welcome-container",
                        Orientation = OrientationTypes.Left,
                        Lookup = "journeys",
                        Content = "Here are a number of resources and guided tours you can select that will educate you of the different developer journeys you can undertake through Fathym."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000022"),
                        Title = "Data Flows",
                        Subtitle = "Limited Trial Tour",
                        Selector = "nide-ide-side-bar .ide-side-bar-action:nth-of-type(2)",
                        Orientation = OrientationTypes.Right,
                        Lookup = "data-flow",
                        Content = "The <b>Data Flow Manager</b> is a powerful drag and drop interface for easily configuring and provisioning end-to-end cloud infrastructure. Navigate here to explore further."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000023"),
                        Title = "Data Applications",
                        Subtitle = "Limited Trial Tour",
                        Selector = "nide-ide-side-bar .ide-side-bar-action:nth-of-type(3)",
                        Orientation = OrientationTypes.Right,
                        Lookup = "data-apps",
                        Content = "<b>Data applications</b> are quick and easy ways to build and deliver enterprise scalable experiences to your users. Create your own, configure your own, or use pre-existing applications."
                    }
                }
            };
        }

        protected virtual GuidedTour createDataApplicationsTour(string lookup)
        {
            return new GuidedTour()
            {
                ID = new Guid("00000000-0000-0000-0000-000000000003"),
                IsFirstTimeViewing = true,
                Lookup = lookup,
                UseOrb = false,
                Steps = new List<GuidedTourStep>()
                {
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000030"),
                        Title = "Data Applications",
                        Subtitle = "Data Applications Tour",
                        Selector = "lcu-limited-trial-data-apps-element .lcu-data-apps-config-manager",
                        Orientation = OrientationTypes.Left,
                        Lookup = "welcome",
                        Content = "<b>Data Applications</b> are quick and easy ways to build and deliver scalable web apps and sites, easily managed for multiple end users."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000031"),
                        Title = "Public/Private Apps Sidebar",
                        Subtitle = "Data Applications Tour",
                        Selector = "lcu-limited-trial-data-apps-element .mat-drawer-inner-container",
                        Orientation = OrientationTypes.Left,
                        Lookup = "apps",
                        Content = "Access your applications here. The examples provided are Freeboard and Fathym Forecaster applications. Applications can be hosted as either public or private. Set your application to private if you want to control access for internal use or set to public if you want to share it with users without a login. You can update visibility settings at any point."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000032"),
                        Title = "View Configuration",
                        Subtitle = "Data Applications Tour",
                        Selector = "lcu-limited-trial-data-apps-element lcu-data-apps-config .mat-tab-group .mat-tab-label:nth-of-type(1)",
                        Orientation = OrientationTypes.Left,
                        Lookup = "view-config",
                        Content = "Here you can view the configuration and version history of your application. Fathym leverages NPM Node Packages to organize and deploy any previous or current version of your application to your custom domain."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000033"),
                        Title = "Application Details",
                        Subtitle = "Data Applications Tour",
                        Selector = "lcu-limited-trial-data-apps-element lcu-data-apps-config .mat-tab-group .mat-tab-label:nth-of-type(2)",
                        Orientation = OrientationTypes.Left,
                        Lookup = "app-config",
                        Content = "Here you can view the name, description and path of the application. The path is the URL where your application is hosted."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000034"),
                        Title = "Add New Data App Button",
                        Subtitle = "Data Applications Tour",
                        Selector = "lcu-limited-trial-data-apps-element #createNewDataAppBtn",
                        Orientation = OrientationTypes.Left,
                        Lookup = "create",
                        Content = "Create and configure your own data app or use pre-existing applications. Fathym has several open source data apps to get you started."
                    }
                }
            };
        }

        protected virtual GuidedTour createDataFlowManagementTour(string lookup)
        {
            return new GuidedTour()
            {
                ID = new Guid("00000000-0000-0000-0000-000000000004"),
                IsFirstTimeViewing = true,
                Lookup = lookup,
                UseOrb = false,
                Steps = new List<GuidedTourStep>()
                {
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000040"),
                        Title = "Data Flow Management",
                        Subtitle = "Data Flow Management Tour",
                        Selector = "lcu-limited-trial-data-flow-element .data-flow-manager-container",
                        Orientation = OrientationTypes.Left,
                        Lookup = "welcome",
                        Content = "Developers can easily manage and create data flows by rapidly configuring and provisioning Azure resources through a visual drag-and-drop interface."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000041"),
                        Title = "Emulated Data Flows",
                        Subtitle = "Data Flow Management Tour",
                        Selector = "lcu-limited-trial-data-flow-element lcu-data-flow-list-element .mat-tab-label:nth-of-type(1)",
                        Orientation = OrientationTypes.Bottom,
                        Lookup = "best-practice",
                        Content = "We have created a sample best practice IoT environment for you to explore using an emulated data flow."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000042"),
                        Title = "Trial Data Flows",
                        Subtitle = "Data Flow Management Tour",
                        Selector = "lcu-limited-trial-data-flow-element lcu-data-flow-list-element .mat-tab-label:nth-of-type(2)",
                        Orientation = OrientationTypes.Bottom,
                        Lookup = "explore",
                        Content = "Use our drag-and-drop interface to explore the tool and connect dummy Azure resources."
                    }
                }
            };
        }

        protected virtual GuidedTour createDataFlowToolTour(string lookup)
        {
            return new GuidedTour()
            {
                ID = new Guid("00000000-0000-0000-0000-000000000005"),
                IsFirstTimeViewing = true,
                Lookup = lookup,
                UseOrb = false,
                Steps = new List<GuidedTourStep>()
                {
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000050"),
                        Title = "Emulator",
                        Subtitle = "Emulated Data Flow Tour",
                        Selector = "lcu-limited-trial-data-flow-element .flowchart-object[data-jtk-node-id='e7457c9c-c9b2-4955-b0a2-330b6244982d']", // selects by attribute selector
                        Orientation = OrientationTypes.Top,
                        Lookup = "emulator",
                        Content = "The <b>emulator</b> is where you can configure your test device data and the frequency that it posts to the ingest. This is ideal for getting data streaming through your IoT infrastructure and into business applications and dashboards, while your hardware team is working to get the actual devices online. Once the real devices are online you can turn off the emulator."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000051"),
                        Title = "Ingest",
                        Subtitle = "Emulated Data Flow Tour",
                        Selector = "lcu-limited-trial-data-flow-element .flowchart-object[data-jtk-node-id='f0e0b225-5e51-44c2-8618-a48a0d7678de']",
                        Orientation = OrientationTypes.Top,
                        Lookup = "ingest",
                        Content = "The <b>ingest</b> is a security-enhanced communication channel for sending and receiving data from your devices or an emulator."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000052"),
                        Title = "Data Map",
                        Subtitle = "Emulated Data Flow Tour",
                        Selector = "lcu-limited-trial-data-flow-element .flowchart-object[data-jtk-node-id='2bb21cb0-37db-4e6e-a762-ab5b1ea3c974']",
                        Orientation = OrientationTypes.Top,
                        Lookup = "data-map",
                        Content = "The <b>data map</b> is a real-time analytics service that allows you to manipulate and analyze your data before pushing to the configured outputs."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000053"),
                        Title = "Cold Storage",
                        Subtitle = "Emulated Data Flow Tour",
                        Selector = "lcu-limited-trial-data-flow-element .flowchart-object[data-jtk-node-id='8a3fe2f2-d7a3-43f5-b8c7-cf87c6691422']",
                        Orientation = OrientationTypes.Top,
                        Lookup = "cold-storage",
                        Content = "This storage option costs less than $1/month. You can send raw, untouched JSON messages from a device to <b>cold storage</b>. This allows you to refer to cold storage for debugging when you need to see the exact messages that were sent from a device."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000054"),
                        Title = "Warm Storage",
                        Subtitle = "Emulated Data Flow Tour",
                        Selector = "lcu-limited-trial-data-flow-element .flowchart-object[data-jtk-node-id='038131c4-57a9-443c-90ff-d683816c2c40']",
                        Orientation = OrientationTypes.Left,
                        Lookup = "warm-storage",
                        Content = "This storage option is more expensive, starting at $25/month. <b>Warm storage</b> is a database, like CosmosDB or SQL Server. It's queryable storage that is ideal for connecting to reporting solutions like Power BI. Fathym helps you convert units (like Celsius to Fahrenheit) before storing in warm storage so the data is ready for downstream use."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000055"),
                        Title = "Hot Storage",
                        Subtitle = "Emulated Data Flow Tour",
                        Selector = "lcu-limited-trial-data-flow-element .flowchart-object[data-jtk-node-id='0392f943-577e-4165-acb8-93c70074c04f']",
                        Orientation = OrientationTypes.Left,
                        Lookup = "hot-storage",
                        Content = "This storage option is also inexpensive, normally less than $1/month. <b>Hot storage</b> is PubSub – Publisher/Subscriber relationships. A publisher application creates and sends messages to a topic. Subscriber applications create a subscription to a topic to receive messages from it. Fathym uses hot storage for real-time sensor dashboards."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000056"),
                        Title = "Warm Query",
                        Subtitle = "Emulated Data Flow Tour",
                        Selector = "lcu-limited-trial-data-flow-element .flowchart-object[data-jtk-node-id='3ccb861c-57f6-44a0-9430-13c68fb19055']",
                        Orientation = OrientationTypes.Top,
                        Lookup = "warm-query",
                        Content = "Depending on the dashboard and reporting tools you’re using, you may not be able to connect directly to the warm storage. If that’s the case, the <b>warm query</b> provides an API endpoint that you can use to get access to the data in warm storage."
                    }
                }
            };
        }

        protected virtual GuidedTour createIoTDeveloperJourneyTour(string lookup)
        {
            return new GuidedTour()
            {
                ID = new Guid("00000000-0000-0000-0000-000000000006"),
                IsFirstTimeViewing = true,
                Lookup = lookup,
                UseOrb = false,
                Steps = new List<GuidedTourStep>()
                {
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000060"),
                        Title = "IoT Developer Journey",
                        Subtitle = "To the Edge and Beyond",
                        Lookup = "iot-journey",
                        Content = "In 6 steps, I’ll guide you how on IoT developers can use the Fathym Low-Code Framework to rapidly provision end-to-end IoT infrastructure and build enterprise scale IoT solutions."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000061"),
                        Title = "Data Flow Manager",
                        Subtitle = "To the Edge and Beyond",
                        Selector = "lcu-limited-trial-data-flow-element .data-flow-manager-container",
                        Orientation = OrientationTypes.Left,
                        Lookup = "data-flow",
                        Content = "The data flow manager is a powerful drag and drop interface for easily configuring and provisioning end-to-end IoT infrastructure."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000062"),
                        Title = "Emulated IoT Data Flow",
                        Subtitle = "To the Edge and Beyond",
                        Selector = "lcu-limited-trial-data-flow-element .data-flow-ide-container",
                        Orientation = OrientationTypes.Left,
                        Lookup = "emulated",
                        Content = "Here is an emulated data flow that demonstrates one of our best practice IoT environments. Using our drag and drop interface, you can create emulated data functions and connect them to data streams and data maps. The data map allows you to manipulate and analyze your data before pushing to various configured storage outputs."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000063"),
                        Title = "Data Applications",
                        Subtitle = "To the Edge and Beyond",
                        Selector = "lcu-limited-trial-data-apps-element .lcu-data-apps-config-manager",
                        Orientation = OrientationTypes.Left,
                        Lookup = "data-apps",
                        Content = "Data applications enable you to build and deliver powerful data-driven web applications and sites that are easily managed for multiple end users."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000064"),
                        Title = "Hosting your Application",
                        Subtitle = "To the Edge and Beyond",
                        Selector = "lcu-data-apps-config #dataAppsConfigCard",
                        Orientation = OrientationTypes.Left,
                        Lookup = "hosting",
                        Content = "You can host and build data apps that integrate automatically with NPM packages and GitHub repositories, enabling you to organize and deploy any previous or current version of your application to your custom domain."
                    },
                    new GuidedTourStep()
                    {
                        ID = new Guid("00000000-0000-0000-0000-000000000065"),
                        Title = "Access Control",
                        Subtitle = "To the Edge and Beyond",
                        Selector = "lcu-data-apps-config #accessControlToggle",
                        Orientation = OrientationTypes.Top,
                        Lookup = "access",
                        Content = "Set your application to private if you want to control access for internal use or set to public if you want to share it with external users without a login. You can update visibility settings at any point."
                    }
                }
            };
        }
        #endregion
    }
}
