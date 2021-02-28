using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace GofancoHDMI
{
	public partial class GofancoHDMI : ServiceBase
	{
		/// <summary>
		/// Initialized automatically by BPUtil.AppInit.
		/// </summary>
		public static Settings settings;
		WebServer server;
		public GofancoHDMI()
		{

			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			server = new WebServer(settings.httpPort, settings.matrixHost, settings.matrixHttpPort);
			server.Start();
		}

		protected override void OnStop()
		{
			server?.Stop();
		}
	}
}
