using System.Numerics;
using System.Runtime.InteropServices;
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Threading.Tasks;

[Library( "dm_dbshotgun", Title = "Double Barrel Shotgun" )]
[Hammer.EditorModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" )]
partial class DBShotgun : BaseDmWeapon
{ 
	// TODO nerf dbshotgun but good
	public override string ViewModelPath => "models/weapons/dbshotgun/v_doublebarrel.vmdl";
	public override float PrimaryRate => 1.2f;
	public override float SecondaryRate => 1.2f;
	public override AmmoType AmmoType => AmmoType.Buckshot;
	public override int ClipSize => 10;
	public override float ReloadTime => 0.5f;
	public override int Bucket => 0;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/weapons/dbshotgun/doublebarrel.vmdl" );

		AmmoClip = 10;
	}

	public override void AttackPrimary()
	{
		// TODO:player pushback
		// applyForce(new Vector3(0, 30, 0));
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
		
		if ( !TakeAmmo( 0 ) )
		{
			DryFire();
			return;
		}

		(Owner as AnimEntity).SetAnimBool( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound("dbshotgun");

		//
		// Shoot the bullets
		//
		ShootBullet( 0.4f, 10.3f, 9.0f, 4.0f, 20);

		Owner.Velocity = Owner.EyeRotation.Forward * -1000;
	}

	public override void AttackSecondary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 0 ) )
		{
			DryFire();
			return;
		}

		(Owner as AnimEntity).SetAnimBool( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "dbshotgun" );

		//
		// Shoot the bullets
		//
		ShootBullet( 0.4f, 10.3f, 9.0f, 4.0f, 20 );

		Owner.Velocity = Owner.EyeRotation.Forward * 1000;
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimBool( "fire", true );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin(2.0f, 3.0f, 4.0f);
		}

		CrosshairPanel?.CreateEvent( "fire" );
	}

	[ClientRpc]
	protected virtual void DoubleShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimBool( "fire_double", true );
		CrosshairPanel?.CreateEvent( "fire" );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin(3.0f, 3.0f, 3.0f);
		}
	}

	public override void OnReloadFinish()
	{
		IsReloading = false;

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( AmmoClip >= ClipSize )
			return;

		if ( Owner is DeathmatchPlayer player )
		{
			var ammo = player.TakeAmmo( AmmoType, 1 );
			if ( ammo == 0 )
				return;

			AmmoClip += ammo;

			if ( AmmoClip < ClipSize )
			{
				Reload();
			}
			else
			{
				FinishReload();
			}
		}
	}

	[ClientRpc]
	protected virtual void FinishReload()
	{
		ViewModelEntity?.SetAnimBool( "reload_finished", true );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
