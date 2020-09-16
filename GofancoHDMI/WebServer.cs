using BPUtil;
using BPUtil.SimpleHttp;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Reflection;

namespace GofancoHDMI
{
	public class WebServer : HttpServer
	{
		string matrixHost;
		int matrixPort;

		public WebServer(int httpPort, string matrixHost, int matrixPort) : base(httpPort)
		{
			// NOTE: The Gofanco HDMI matrix doesn't actually speak HTTP properly. It likely has numerous vulnerabilities and little effort is made by this application to isolate the HDMI matrix from malicious input.
			this.matrixHost = matrixHost;
			this.matrixPort = matrixPort;
		}

		public override void handleGETRequest(HttpProcessor p)
		{
			string pageLower = p.requestedPage.ToLower();
			if (pageLower == "read")
			{
				MatrixStatus m = new MatrixStatus();
				m.status = ReadStatus();
				m.mapNames = ReadMapNames();

				p.writeSuccess("application/json");
				p.outputStream.Write(JsonConvert.SerializeObject(m, Formatting.Indented));
			}
			else if (pageLower == "setoutput")
			{
				SetOutput(p, "1");
				SetOutput(p, "2");
				SetOutput(p, "3");
				SetOutput(p, "4");
				p.writeSuccess();
			}
			else if (pageLower == "savemap")
			{
				int map = p.GetIntParam("map", -1);
				if (map >= 1 || map <= 8)
				{
					POSTString("application/x-www-form-urlencoded",
						"save=" + map);
				}
				p.writeSuccess();
			}
			else if (pageLower == "loadmap")
			{
				int map = p.GetIntParam("map", -1);
				if (map >= 1 || map <= 8)
				{
					POSTString("application/x-www-form-urlencoded",
						"call=" + map);
				}
				p.writeSuccess();
			}
			else if (pageLower == "setionames")
			{
				POSTString("application/x-www-form-urlencoded",
					"namein1?" + Len7(p.GetParam("in1")) + "?"
					+ "namein2?" + Len7(p.GetParam("in2")) + "?"
					+ "namein3?" + Len7(p.GetParam("in3")) + "?"
					+ "namein4?" + Len7(p.GetParam("in4")) + "?"
					+ "nameout1?" + Len7(p.GetParam("out1")) + "?"
					+ "nameout2?" + Len7(p.GetParam("out2")) + "?"
					+ "nameout3?" + Len7(p.GetParam("out3")) + "?"
					+ "nameout4?" + Len7(p.GetParam("out4")) + "?");
				p.writeSuccess();
			}
			else if (pageLower == "setmapnames")
			{
				POSTString("application/x-www-form-urlencoded",
					"mname1?" + Len7(p.GetParam("1")) + "?"
					+ "mname2?" + Len7(p.GetParam("2")) + "?"
					+ "mname3?" + Len7(p.GetParam("3")) + "?"
					+ "mname4?" + Len7(p.GetParam("4")) + "?"
					+ "mname5?" + Len7(p.GetParam("5")) + "?"
					+ "mname6?" + Len7(p.GetParam("6")) + "?"
					+ "mname7?" + Len7(p.GetParam("7")) + "?"
					+ "mname8?" + Len7(p.GetParam("8")) + "?");
				p.writeSuccess();
			}
			else
			{
				MatrixStatus m = new MatrixStatus();
				m.status = ReadStatus();
				m.mapNames = ReadMapNames();

				p.writeSuccess();
				p.outputStream.WriteLine("<html>");
				p.outputStream.WriteLine("<head>");
				p.outputStream.WriteLine("<style type=\"text/css\">");
				p.outputStream.WriteLine("body { font-family: sans-serif; }");
				p.outputStream.WriteLine("code { font-family: Consolas, monospace; display: block; white-space: pre-wrap; border: 1px solid #EFEFEF; background-color: #F6F6F6; padding: 3px 6px; }");
				p.outputStream.WriteLine("li { margin-top: 10px; }");
				p.outputStream.WriteLine("</style>");
				p.outputStream.WriteLine("<title>GofancoHDMI Web Control " + Globals.AssemblyVersion + "</title>");
				p.outputStream.WriteLine("</head>");
				p.outputStream.WriteLine("<body>");
				p.outputStream.WriteLine("<h1>GofancoHDMI Web Control Version " + Globals.AssemblyVersion + "</h1>");
				p.outputStream.WriteLine("<p>");
				p.outputStream.WriteLine("Designed for Gofanco PRO-Matrix44-SC running \"Software version: 4x4_SW_1.1\"");
				p.outputStream.WriteLine("</p>");
				p.outputStream.WriteLine("</p>");
				p.outputStream.WriteLine("<h2>Commands</h2>");
				p.outputStream.WriteLine("<p>");
				p.outputStream.WriteLine("<ul>");

				OutputListItemLink(p, "read", "Returns current configuration in JSON format.");

				OutputListItemLink(p, "setoutput?1=0", "Turns off output 1.");
				OutputListItemLink(p, "setoutput?1=3", "Sets output 1 to input 3.");
				OutputListItemLink(p, "setoutput?1=2&2=3&4=2", "Sets output 1 to input 2, output 2 to input 3, and output 4 to input 2.");

				OutputListItemLink(p, "savemap?map=1", "Saves current input/output state in mapping slot 1. (1-8 available)");
				OutputListItemLink(p, "loadmap?map=1", "Saves current input/output state in mapping slot 1. (1-8 available)");

				if (m.status != null)
				{
					string setNamesUrl = "setionames?in1=" + Uri.EscapeDataString(m.status.namein1)
						+ "&in2=" + Uri.EscapeDataString(m.status.namein2)
						+ "&in3=" + Uri.EscapeDataString(m.status.namein3)
						+ "&in4=" + Uri.EscapeDataString(m.status.namein4)
						+ "&out1=" + Uri.EscapeDataString(m.status.nameout1)
						+ "&out2=" + Uri.EscapeDataString(m.status.nameout2)
						+ "&out3=" + Uri.EscapeDataString(m.status.nameout3)
						+ "&out4=" + Uri.EscapeDataString(m.status.nameout4);
					OutputListItemLink(p, setNamesUrl, "Sets names of all inputs and outputs. Omitted names are set to empty strings. Each name is limited to 7 characters.");
				}
				else
					p.outputStream.WriteLine("<li>Basic device status failed to load.</li>");

				if (m.mapNames != null)
				{
					string setMapNamesUrl = "setmapnames"
						+ "?1=" + Uri.EscapeDataString(m.mapNames.namem1)
						+ "&2=" + Uri.EscapeDataString(m.mapNames.namem2)
						+ "&3=" + Uri.EscapeDataString(m.mapNames.namem3)
						+ "&4=" + Uri.EscapeDataString(m.mapNames.namem4)
						+ "&5=" + Uri.EscapeDataString(m.mapNames.namem5)
						+ "&6=" + Uri.EscapeDataString(m.mapNames.namem6)
						+ "&7=" + Uri.EscapeDataString(m.mapNames.namem7)
						+ "&8=" + Uri.EscapeDataString(m.mapNames.namem8);
					OutputListItemLink(p, setMapNamesUrl, " Sets names of all input/output mappings. Omitted names are set to empty strings. Each name is limited to 7 characters.");
				}
				else
					p.outputStream.WriteLine("<li>Map names failed to load.</li>");

				p.outputStream.WriteLine("</ul>");
				p.outputStream.WriteLine("</p>");
				p.outputStream.WriteLine("<h2>Current Configuration</h2>");
				p.outputStream.WriteLine("<p>");
				p.outputStream.WriteLine("<code>");
				p.outputStream.WriteLine(JsonConvert.SerializeObject(m, Formatting.Indented));
				p.outputStream.WriteLine("</code>");
				p.outputStream.WriteLine("</body>");
				p.outputStream.WriteLine("</html>");
			}
		}

		private void OutputListItemLink(HttpProcessor p, string url, string description)
		{
			p.outputStream.WriteLine("<li><a href=\"" + StringUtil.HtmlAttributeEncode(url) + "\" target=\"_blank\">" + url + "</a> - " + description + "</li>");
		}

		private bool SetOutput(HttpProcessor p, string outputName)
		{
			int value = p.GetIntParam(outputName, -1);
			if (value >= 0 && value <= 4)
			{
				POSTString("application/x-www-form-urlencoded", "out" + outputName + "=" + value);
				return true;
			}
			return false;
		}

		private string Len7(string str)
		{
			if (str == null)
				return "";
			else if (str.Length > 7)
				return str.Substring(0, 7);
			else
				return str;
		}

		private string POSTString(string contentType, string str)
		{
			byte[] postdata = ByteUtil.Utf8NoBOM.GetBytes(str);
			using (TcpClient tcpc = new TcpClient(matrixHost, matrixPort))
			{
				NetworkStream ns = tcpc.GetStream();
				ByteUtil.WriteUtf8("POST /inform.cgi?undefined HTTP/1.1\r\n", ns);
				ByteUtil.WriteUtf8("Host: " + matrixHost + "\r\n", ns);
				ByteUtil.WriteUtf8("Content-Length: " + postdata.Length + "\r\n", ns);
				ByteUtil.WriteUtf8("Content-Type: " + contentType + "\r\n", ns);
				ByteUtil.WriteUtf8("User-Agent: GofancoHDMI Web Control " + Globals.AssemblyVersion + "\r\n", ns);
				ByteUtil.WriteUtf8("\r\n", ns);

				ns.Write(postdata, 0, postdata.Length);

				using (StreamReader sr = new StreamReader(ns, ByteUtil.Utf8NoBOM, false, 8196, true))
					return sr.ReadToEnd();
			}
		}

		private BasicStatus ReadStatus()
		{
			string response = POSTString("application/json;charset=UTF-8", "{\"param1\":\"1\"}");
			if (response == null)
				return null;
			return JsonConvert.DeserializeObject<BasicStatus>(response);
		}
		private MapNames ReadMapNames()
		{
			string response = POSTString("application/x-www-form-urlencoded", "LOADMAP");
			if (response == null)
				return null;
			return JsonConvert.DeserializeObject<MapNames>(response);

		}
		public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
		{
			handleGETRequest(p);
		}

		protected override void stopServer()
		{
		}
	}
	public class MatrixStatus
	{
		public MapNames mapNames;
		public BasicStatus status;
	}
	public class MapNames
	{
		public string namem1;
		public string namem2;
		public string namem3;
		public string namem4;
		public string namem5;
		public string namem6;
		public string namem7;
		public string namem8;
	}
	public class BasicStatus
	{
		public long out1;
		public long out2;
		public long out3;
		public long out4;
		public string namein1;
		public string namein2;
		public string namein3;
		public string namein4;
		public string nameout1;
		public string nameout2;
		public string nameout3;
		public string nameout4;
		public long powstatus;
	}
}