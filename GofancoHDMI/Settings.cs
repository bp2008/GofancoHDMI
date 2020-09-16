using BPUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GofancoHDMI
{
	public class Settings : SerializableObjectBase
	{
		public int httpPort = 80;
		public string matrixHost = "192.168.1.70";
		public int matrixHttpPort = 80;
	}
}
