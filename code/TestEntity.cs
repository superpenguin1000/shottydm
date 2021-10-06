using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class TestEntity : Entity
{
	[Net] public string MyString { get; set; }

	public override void Spawn()
	{
		MyString = $"Random Number Is: {Rand.Int( 1, 3 )}";
		Transmit = TransmitType.Always;

		base.Spawn();
	}

	public override void ClientSpawn()
	{
		Log.Info( "Spawned on Client: " + MyString );
		base.ClientSpawn();
	}
}
