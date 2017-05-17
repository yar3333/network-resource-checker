namespace NetworkResourceChecker
{
	public class KnownPort
	{
		public string name { get; set; }
		public int port { get; set; }

		public KnownPort(string name, int port)
		{
			this.name = name;
			this.port = port;
		}
	}
}