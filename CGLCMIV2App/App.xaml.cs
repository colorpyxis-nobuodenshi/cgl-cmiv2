using CGLCMIV2.Application;
using CGLCMIV2.Device;
using CGLCMIV2.Domain;
using CGLCMIV2.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CGLCMIV2App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            //base.OnStartup(e);

            var builder = new ConfigurationBuilder()
               .SetBasePath(System.IO.Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            Configuration = builder.Build();

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            //
            ServiceProvider.GetService<AppLifeCycle>().Run();
            ServiceProvider.GetService<DeviceMonitorService>().Run();
            ServiceProvider.GetService<FileOutputService>().Run();

            ServiceProvider.GetRequiredService<MainWindow>().Show();

            ServiceProvider.GetService<ILogger>().Infomation("application start.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //base.OnExit(e);
            ServiceProvider.GetService<AppStore>().Store();

            ServiceProvider.GetService<ILogger>().Infomation("application end.");
        }

        void ConfigureServices(IServiceCollection services)
        {
            var settings = new AppSettings();
            Configuration.Bind(settings);
            
            services.AddSingleton(settings);
            services.AddSingleton(settings.MeasureResultOutputOption);

            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<ICamera, DFK33UX264>();
            services.AddSingleton<IAutoStage, DS112>();
            services.AddSingleton<ILEDLight, LEDLight>();
            //services.AddSingleton<ICamera, CameraMock>();
            //services.AddSingleton<IAutoStage, AutoStageMock>();
            //services.AddSingleton<ILEDLight, LEDControllerMock>();
            services.AddSingleton<IColorGradingConditonFileLoader, ColorGradingConditonFileLoader>();
            services.AddSingleton<AppLifeCycle>();
            services.AddSingleton<IColorimetry, Colorimetry>();
            services.AddSingleton<IColorGradeJudgement, ColorGradeJudgement>();
            services.AddSingleton<IMeasureResultWriter, MeasureResultWriter>();
            services.AddSingleton<IWhitepointWriter, WhitepointWriter>();
            services.AddSingleton<IPixelsTiffFileStore, PixelsTiffFileStore>();
            services.AddSingleton<IPixelsTiffFileLoader, PixelsTiffFileLoader>();
            services.AddSingleton<IInstrumentalErrorCorrectionMatrixFileLoader, InstrumentalErrorCorrectionMatrixFileLoader>();
            services.AddSingleton<AppStore>();

            //
            services.AddSingleton<DeviceMonitorService>();
            services.AddSingleton<FileOutputService>();

            //
            services.AddTransient<ConnectHardware>();
            services.AddTransient<DisconnectHardware>();
            services.AddTransient<Start>();
            services.AddTransient<Stop>();
            services.AddTransient<Reset>();
            services.AddTransient<Abort>();
            services.AddTransient<CameraCaptureStart>();
            services.AddTransient<CameraCaptureStop>();
            services.AddTransient<CameraCaptureSnap>();
            services.AddTransient<AutoStageMoveORG>();
            services.AddTransient<AutoStageMoveHome>();
            services.AddTransient<AutoStageRotateCW45>();
            services.AddTransient<AutoStageRotateCCW45>();
            services.AddTransient<AutoStageRotateHome>();
            services.AddTransient<AutoStageMoveMeasurePoint>();
            services.AddTransient<AutoStageMoveWorkSetPoint>();
            services.AddTransient<AutoStageMoveReplacementPoint>();
            services.AddTransient<AutoStageMoveMeasurePointOnSpectralon>();
            services.AddTransient<AutoStageRotateCWJ>();
            services.AddTransient<AutoStageRotateCCWJ>();
            services.AddTransient<MakeShadingData>();
            services.AddTransient<ScanWhitepoint>();
            services.AddTransient<ScanWhitepointOnSpectralon>();
            services.AddTransient<LEDPowerChange>();
            services.AddTransient<BarcodeRead>();

            services.AddSingleton<AutoColorMeasuringStart>();
            services.AddSingleton<AutoGradingStart>();
            services.AddSingleton<ColorMeasuringStart>();

            services.AddTransient<MainWindow>();
            services.AddTransient<IMessageWindow, MessageWindow>();
        }
    }
}
