using Sandbox;

[Library( "sdm_shotgunbutt", Title = "Shotgun Butt", Spawnable = true )]
[Hammer.EditorModel( "" )]

partial class ShotgunButt : BaseDmWeapon
{
	public override string ViewModelPath => "models/weapons/shotgunbutt/v_shotgunbutt.vmdl";
	public override int Bucket => 1;
	public override float PrimaryRate => 2.0f;



	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/weapons/shotgunbutt/shotgunbutt.vmdl" );

		AmmoClip = 1;
	}

	public override bool CanReload()
	{
		return false;
	}

	public override void AttackPrimary()
	{
		if ( MeleeAttack() )
		{
			OnMeleeHit();
		}
		else
		{
			OnMeleeMiss();
		}

		PlaySound( "rust_flashlight.attack" );
	}

	private bool MeleeAttack()
	{
		var forward = Owner.EyeRotation.Forward;
		forward = forward.Normal;

		bool hit = false;

		foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 80, 20.0f ) )
		{
			if ( !tr.Entity.IsValid() ) continue;

			tr.Surface.DoBulletImpact( tr );

			hit = true;

			if ( !IsServer ) continue;

			using ( Prediction.Off() )
			{
				if(Owner.Velocity.x > 500 || Owner.Velocity.y > 500 || Owner.Velocity.z > 500 )
				{
					var damageInfo2 = DamageInfo.FromBullet( tr.EndPos, forward * 100, 100)
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo2 );
				}
				else
				{
					var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100, 25 )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

					tr.Entity.TakeDamage( damageInfo );
				}
			}
		}

		return hit;
	}

	[ClientRpc]
	private void OnMeleeMiss()
	{
		Host.AssertClient();

		if ( IsLocalPawn )
		{
			_ = new Sandbox.ScreenShake.Perlin();
		}

		ViewModelEntity?.SetAnimBool( "attack", true );
	}

	[ClientRpc]
	private void OnMeleeHit()
	{
		Host.AssertClient();

		if ( IsLocalPawn )
		{
			_ = new Sandbox.ScreenShake.Perlin( 1.0f, 1.0f, 3.0f );
		}

		ViewModelEntity?.SetAnimBool( "attack_hit", true );
	}
}

