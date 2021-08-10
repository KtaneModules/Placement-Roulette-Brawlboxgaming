using System;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using KModkit;
using UnityEngine;
using Rnd = UnityEngine.Random;

/// <summary>
/// On the Subject of Placement Roulette
/// Created by Brawlboxgaming
/// </summary>
public class PlacementRouletteModule : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMAudio Audio;
    public KMSelectable Left, Right, LeftSubmit, RightSubmit, Submit;
    public KMRuleSeedable RuleSeedable;
    public TextMesh InformationText;
    public Texture[] ItemTextures, PlacementTextures;
    public MeshRenderer[] ItemRenderers, PlacementRenderers;
    public MeshRenderer SubmissionIcon;
    public GameObject ItemRoulette;

    private int infoIx = 0;
    private int subIx = 11;
    private string[] Information;
    private int Answer;
    private static readonly int[][] Seed1ItemTable = {new[]{8,8,8,14,11,14,11,12,12,5,5,16},
                                 new[]{6,8,6,6,12,14,15,15,5,5,7,5},
                                 new[]{0,14,8,14,11,6,15,7,12,12,7,7},
                                 new[]{14,0,14,11,12,11,12,12,12,17,17,5},
                                 new[]{8,6,11,12,4,3,7,17,7,12,5,16},
                                 new[]{6,14,12,4,10,4,7,10,12,12,7,7},
                                 new[]{0,6,4,10,13,15,12,17,17,17,17,7},
                                 new[]{0,14,1,14,2,15,12,12,17,12,5,7},
                                 new[]{1,11,9,2,1,9,10,7,7,17,17,16},
                                 new[]{0,1,14,15,9,10,17,12,7,7,5,5},
                                 new[]{1,1,14,9,15,12,2,11,15,7,17,5}};
    private int[][] ItemTable;
    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private bool isSolved;
    private Coroutine Spinner;
    private KMAudio.KMAudioRef SpinSound;

    // For Souvenir.
    private string VehicleType, Character, Vehicle, Drift, TrackType, Track;

    void Start()
    {
        _moduleId = _moduleIdCounter++;

        for (int i = 0; i < ItemRenderers.Length; i++)
        {
            ItemRenderers[i].material.mainTexture = ItemTextures[Rnd.Range(0, ItemTextures.Length)];
        }

        var Placement = Rnd.Range(0, PlacementTextures.Length);

        PlacementRenderers[0].material.mainTexture = PlacementTextures[Placement];

        var random = RuleSeedable.GetRNG();
        if (random.Seed == 1)
            ItemTable = Seed1ItemTable;
        else
        {
            ItemTable = new int[11][];
            for (int i = 0; i < 11; i++)
            {
                ItemTable[i] = new int[12];
                for (int j = 0; j < 12; j++)
                {
                    ItemTable[i][j] = random.Next(0, 18);
                }
            }
        }

        var charsLight = new[] { "Baby Mario", "Baby Luigi", "Baby Peach", "Baby Daisy", "Toad", "Toadette", "Koopa Troopa", "Dry Bones" };
        var charsMedium = new[] { "Mario", "Luigi", "Peach", "Daisy", "Yoshi", "Birdo", "Diddy Kong", "Bowser Jr.", "Mii" };
        var charsHeavy = new[] { "Wario", "Waluigi", "Donkey Kong", "Bowser", "King Boo", "Rosalina", "Funky Kong", "Dry Bowser" };
        var chars = new[] { charsLight, charsMedium, charsHeavy };

        var vehiclesLight = new[] { "Standard Kart S", "Baby Booster", "Concerto", "Cheep Charger", "Rally Romper", "Blue Falcon", "Standard Bike S", "Bullet Bike", "Nanobike", "Quacker", "Magikruiser", "Bubble Bike" };
        var vehiclesMedium = new[] { "Standard Kart M", "Nostalgia 1", "Wild Wing", "Turbo Blooper", "Royal Racer", "B Dasher Mk. 2", "Standard Bike M", "Mach Bike", "Bon Bon", "Rapide", "Nitrocycle", "Dolphin Dasher" };
        var vehiclesHeavy = new[] { "Standard Kart L", "Offroader", "Flame Flyer", "Piranha Prowler", "Jetsetter", "Honeycoupe", "Standard Bike L", "Bowser Bike", "Wario Bike", "Twinkle Star", "Torpedo", "Phantom" };
        var vehicles = new[] { vehiclesLight, vehiclesMedium, vehiclesHeavy };

        var weights = new[] { "Lightweight", "Mediumweight", "Heavyweight" };
        var ix = Rnd.Range(0, 3);
        var Weight = weights[ix];

        var vTypes = new[] { "Kart", "Bike" };
        int vehicleix = Rnd.Range(0, 2);
        VehicleType = vTypes[vehicleix];
        Character = chars[ix][Rnd.Range(0, 8)];
        Vehicle = vehicles[ix][Rnd.Range(0, 6) + vehicleix * 6];

        var drifts = new[] { "Manual", "Automatic" };
        Drift = drifts[Rnd.Range(0, 2)];

        var tracks = new[] { "Luigi Circuit", "Moo Moo Meadows", "Mushroom Gorge", "Toad's Factory", "Mario Circuit", "Coconut Mall", "DK Snowboard Cross", "Wario's Gold Mine", "Daisy Circuit", "Koopa Cape", "Maple Treeway", "Grumble Volcano", "Dry Dry Ruins", "Moonview Highway", "Bowser's Castle", "Rainbow Road", "GCN Peach Beach", "DS Yoshi Falls", "SNES Ghost Valley 2", "N64 Mario Raceway", "N64 Sherbet Land", "GBA Shy Guy Beach", "DS Delfino Square", "GCN Waluigi Stadium", "DS Desert Hills", "GBA Bowser Castle 3", "N64 DK's Jungle Parkway", "GCN Mario Circuit", "SNES Mario Circuit 3", "DS Peach Gardens", "GCN DK Mountain", "N64 Bowser's Castle" };
        int trackix = Rnd.Range(0, 2);
        var tTypes = new[] { "Nitro", "Retro" };
        TrackType = tTypes[trackix];
        Track = tracks[Rnd.Range(0, 16) + trackix * 16];

        Information = new[] { Character, Vehicle, Drift, Track };

        int weightOffset = 0;
        int vehicleTypeOffset = 0;
        int driftOffset = 0;
        int trackTypeOffset = 0;
        int batteryCountOffset = 0;
        int indicatorOffset = 0;
        int Offset = 0;

        string ind = "";

        if (random.Seed == 1)
        {
            switch (Weight)
            {
                case "Lightweight":
                    Offset += 3;
                    weightOffset = 3;
                    break;
                case "Mediumweight":
                    Offset += 2;
                    weightOffset = 2;
                    break;
                case "Heavyweight":
                    Offset += 1;
                    weightOffset = 1;
                    break;
            }
            switch (VehicleType)
            {
                case "Bike":
                    Offset += 1;
                    vehicleTypeOffset = 1;
                    break;
                case "Kart":
                    Offset += 2;
                    vehicleTypeOffset = 2;
                    break;
            }
            switch (Drift)
            {
                case "Manual":
                    Offset += 1;
                    driftOffset = 1;
                    break;
                case "Automatic":
                    Offset += 3;
                    driftOffset = 3;
                    break;
            }
            switch (TrackType)
            {
                case "Nitro":
                    Offset += 3;
                    trackTypeOffset = 3;
                    break;
                case "Retro":
                    Offset += 2;
                    trackTypeOffset = 2;
                    break;
            }

            if (Bomb.GetBatteryCount() > 3)
            {
                Offset += 2;
                batteryCountOffset = 2;
            }
            else
            {
                Offset += 1;
                batteryCountOffset = 1;
            }

            ind = "CAR";

            if (Bomb.IsIndicatorPresent(ind))
            {
                Offset += 3;
                indicatorOffset = 3;
            }
        }

        else
        {
            var indicators = new[] { "SND", "CLR", "CAR", "IND", "FRQ", "SIG", "NSA", "MSA", "TRN", "BOB", "FRK" };
            ind = indicators[random.Next(0, 11)];
            int batteries = random.Next(2, 6);
            int[] numbers = new[] { 1, 2, 3, 1, 2, 3, 1, 2, 3, 1, 2, 3 };
            random.ShuffleFisherYates(numbers);
            switch (Weight)
            {
                case "Lightweight":
                    Offset += numbers[0];
                    weightOffset = numbers[0];
                    break;
                case "Mediumweight":
                    Offset += numbers[1];
                    weightOffset = numbers[1];
                    break;
                case "Heavyweight":
                    Offset += numbers[2];
                    weightOffset = numbers[2];
                    break;
            }
            switch (VehicleType)
            {
                case "Bike":
                    Offset += numbers[3];
                    vehicleTypeOffset = numbers[3];
                    break;
                case "Kart":
                    Offset += numbers[4];
                    vehicleTypeOffset = numbers[4];
                    break;
            }
            switch (Drift)
            {
                case "Manual":
                    Offset += numbers[5];
                    driftOffset = numbers[5];
                    break;
                case "Automatic":
                    Offset += numbers[6];
                    driftOffset = numbers[6];
                    break;
            }
            switch (TrackType)
            {
                case "Nitro":
                    Offset += numbers[7];
                    trackTypeOffset = numbers[7];
                    break;
                case "Retro":
                    Offset += numbers[8];
                    trackTypeOffset = numbers[8];
                    break;
            }

            if (Bomb.GetBatteryCount() > batteries)
            {
                Offset += numbers[9];
                batteryCountOffset = numbers[9];
            }
            else
            {
                Offset += numbers[10];
                batteryCountOffset = numbers[10];
            }

            if (Bomb.IsIndicatorPresent(ind))
            {
                Offset += numbers[11];
                indicatorOffset = numbers[11];
            }
        }

        InformationText.text = Information[infoIx];
        ItemRenderers[0].gameObject.SetActive(false);

        Left.OnInteract += LeftPressed;
        Right.OnInteract += RightPressed;

        LeftSubmit.OnInteract += LeftSubmitPressed;
        RightSubmit.OnInteract += RightSubmitPressed;

        Submit.OnInteract += SubmitPressed;

        Answer = ItemTable[Offset - 6][Placement];

        Debug.LogFormat("[Placement Roulette #{0}] Using rule seed: {1}", _moduleId, random.Seed);
        Debug.LogFormat("[Placement Roulette #{0}] Placement: {1}", _moduleId, Placement + 1);
        Debug.LogFormat("[Placement Roulette #{0}] Character Weight Class: {1} (+{2})", _moduleId, Weight, weightOffset);
        Debug.LogFormat("[Placement Roulette #{0}] Vehicle Type: {1} (+{2})", _moduleId, VehicleType, vehicleTypeOffset);
        Debug.LogFormat("[Placement Roulette #{0}] Drift: {1} (+{2})", _moduleId, Information[2], driftOffset);
        Debug.LogFormat("[Placement Roulette #{0}] Track Type: {1} (+{2})", _moduleId, TrackType, trackTypeOffset);
        Debug.LogFormat("[Placement Roulette #{0}] Battery Count: {1} (+{2})", _moduleId, Bomb.GetBatteryCount(), batteryCountOffset);
        Debug.LogFormat("[Placement Roulette #{0}] Is {3} present: {1} (+{2})", _moduleId, Bomb.IsIndicatorPresent(ind), indicatorOffset, ind);
        Debug.LogFormat("[Placement Roulette #{0}] Final Offset: {1}", _moduleId, Offset);
        Debug.LogFormat("[Placement Roulette #{0}] The correct answer is {1}.", _moduleId, ItemTextures[Answer].name);
    }

    private bool LeftPressed()
    {
        ButtonHandler(Left, () =>
        {
            infoIx = (infoIx + Information.Length - 1) % Information.Length;
            InformationText.text = Information[infoIx];
        });
        return false;
    }

    private bool RightPressed()
    {
        ButtonHandler(Right, () =>
        {
            infoIx = (infoIx + 1) % Information.Length;
            InformationText.text = Information[infoIx];
        });
        return false;
    }

    private bool LeftSubmitPressed()
    {
        ButtonHandler(LeftSubmit, () =>
        {
            subIx = (subIx + ItemTextures.Length - 1) % ItemTextures.Length;
            SubmissionIcon.material.mainTexture = ItemTextures[subIx];
        });
        return false;
    }

    private bool RightSubmitPressed()
    {
        ButtonHandler(RightSubmit, () =>
        {
            subIx = (subIx + 1) % ItemTextures.Length;
            SubmissionIcon.material.mainTexture = ItemTextures[subIx];
        });
        return false;
    }

    private bool SubmitPressed()
    {
        ButtonHandler(Submit, () =>
        {
            if (Spinner != null)
                StopCoroutine(Spinner);
            Spinner = StartCoroutine(RouletteSpin(Answer == subIx, subIx));
        });
        return false;
    }

    private IEnumerator RouletteSpin(bool solved, int inputIx)
    {
        var elapsed = 0f;
        var duration = 3.4f;
        var currentRotation = 0;
        var endRotation = 2700f;

        if (SpinSound != null)
            SpinSound.StopSound();

        SpinSound = Audio.HandlePlaySoundAtTransformWithRef("Roulette", transform, false);

        int lastFace = 0;

        ItemRenderers[0].gameObject.SetActive(true);

        while (elapsed < duration)
        {
            var t = elapsed / duration;
            var angle = Mathf.Lerp(currentRotation, endRotation, -t * (t - 2));
            ItemRoulette.transform.localEulerAngles = new Vector3(angle, 0, 0);
            var newFace = (int)(angle / 90);
            if (newFace != lastFace)
            {
                ItemRenderers[(newFace + 2) % 4].material.mainTexture = ItemTextures[newFace == 28 ? inputIx : Rnd.Range(0, ItemTextures.Length)];
                lastFace = newFace;
            }
            yield return null;
            elapsed += Time.deltaTime;
        }
        ItemRoulette.transform.localEulerAngles = new Vector3(endRotation, 0, 0);

        if (solved)
        {
            Module.HandlePass();
            isSolved = true;
            InformationText.text = "";
            Debug.LogFormat("[Placement Roulette #{0}] Module solved!", _moduleId);
        }
        else
        {
            Module.HandleStrike();
            Debug.LogFormat("[Placement Roulette #{0}] The submitted answer was {1}. The correct answer was {2}. Strike!", _moduleId, ItemTextures[inputIx].name, ItemTextures[Answer].name);
        }
        Spinner = null;

        yield return new WaitForSeconds(1.1f);
    }

    private void ButtonHandler(KMSelectable btn, Action action)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btn.transform);
        btn.AddInteractionPunch();
        if (isSolved)
            return;
        action();
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} cycle | !{0} submit <icon> [icon names are: Banana, Banana3, Blooper, Blue, Bomb, Bullet, FIB, Golden, Green, Green3, Mega, Mushroom, Mushroom3, POW, Red, Red3, Shock, Star]";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*cycle\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            for (int i = 0; i < 3; i++)
            {
                Right.OnInteract();
                yield return new WaitForSeconds(2f);
            }
            Right.OnInteract();
            yield break;
        }

        var match = Regex.Match(command, @"^\s*submit\s+(.*?)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        if (match.Success)
        {
            var texture = ItemTextures.IndexOf(tx => string.Equals(tx.name, match.Groups[1].Value, StringComparison.InvariantCultureIgnoreCase));
            if (texture == -1)
                yield break;
            yield return null;
            while (subIx != texture)
            {
                RightSubmit.OnInteract();
                yield return new WaitForSeconds(.2f);
            }
            Submit.OnInteract();
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (subIx != Answer)
        {
            RightSubmit.OnInteract();
            yield return new WaitForSeconds(.2f);
        }
        Submit.OnInteract();
        while (!isSolved)
            yield return true;
    }
}
