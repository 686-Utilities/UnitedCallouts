﻿namespace UnitedCallouts.Callouts;

[CalloutInfo("[UC] Stolen Truck Pursuit", CalloutProbability.Medium)]
public class StolenTruckPursuit : Callout
{
    private static readonly string[] TruckList = { "Biff", "Mixer", "Hauler", "Mule", "Flatbed", "Packer", "Pounder" };
    private static Ped _suspect;
    private static Vehicle _truck;
    private static Vector3 _spawnPoint;
    private static Blip _blip;
    private static LHandle _pursuit;
    private static bool _pursuitCreated;
    private static bool _hasBackupBeenCalled;
    
    public override bool OnBeforeCalloutDisplayed()
    {
        _spawnPoint = World.GetNextPositionOnStreet(MainPlayer.Position.Around(1000f));
        ShowCalloutAreaBlipBeforeAccepting(_spawnPoint, 50f);
        CalloutMessage = "[UC]~w~ Reports of a Stolen Truck Pursuit.";
        CalloutPosition = _spawnPoint;
        Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_GRAND_THEFT_AUTO IN_OR_ON_POSITION", _spawnPoint);
        return base.OnBeforeCalloutDisplayed();
    }

    public override bool OnCalloutAccepted()
    {
        Game.LogTrivial("UnitedCallouts Log: StolenTruck callout accepted.");
        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~UnitedCallouts",
            "~y~Stolen Truck Pursuit", "~b~Dispatch: ~w~A Truck was stolen. Respond with ~r~Code 3");

        _truck = new(TruckList[Rndm.Next(TruckList.Length)], _spawnPoint)
        {
            IsPersistent = true
        };

        _suspect = _truck.CreateRandomDriver();
        _suspect.BlockPermanentEvents = true;
        _suspect.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.Emergency);

        _blip = _suspect.AttachBlip();
        _blip.IsFriendly = false;

        return base.OnCalloutAccepted();
    }

    public override void Process()
    {
        if (!_pursuitCreated && MainPlayer.DistanceTo(_suspect.Position) < 30f)
        {
            _pursuit = Functions.CreatePursuit();
            Functions.AddPedToPursuit(_pursuit, _suspect);
            Functions.SetPursuitIsActiveForPlayer(_pursuit, true);

            
            if (!_hasBackupBeenCalled && Settings.ActivateAiBackup)
            {
                Functions.RequestBackup(_spawnPoint, LSPD_First_Response.EBackupResponseType.Pursuit,
                    LSPD_First_Response.EBackupUnitType.LocalUnit);
                Functions.RequestBackup(_spawnPoint, LSPD_First_Response.EBackupResponseType.Pursuit,
                    LSPD_First_Response.EBackupUnitType.LocalUnit);
                Functions.RequestBackup(_spawnPoint, LSPD_First_Response.EBackupResponseType.Pursuit,
                    LSPD_First_Response.EBackupUnitType.AirUnit);
                _hasBackupBeenCalled = true;
            }
            else
            {
                Settings.ActivateAiBackup = false;
            }

            _pursuitCreated = true;
        }

        if (MainPlayer.IsDead) End();
        if (Game.IsKeyDown(Settings.EndCall)) End();
        if (_suspect && _suspect.IsDead) End();
        if (_suspect && Functions.IsPedArrested(_suspect)) End();
        base.Process();
    }

    public override void End()
    {
        if (_suspect) _suspect.Dismiss();
        if (_truck) _truck.Dismiss();
        if (_blip) _blip.Delete();
        Game.DisplayNotification("web_lossantospolicedept", "web_lossantospolicedept", "~w~UnitedCallouts",
            "~y~Stolen Truck Pursuit", "~b~You: ~w~Dispatch we're code 4. Show me ~g~10-8.");
        Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH ALL_UNITS_CODE4 NO_FURTHER_UNITS_REQUIRED");
        base.End();
    }
}