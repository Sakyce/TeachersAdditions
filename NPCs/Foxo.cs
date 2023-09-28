using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

namespace Additions.Characters
{
    public class Foxo : Teacher
    {
        private const float slapSpeed = 100f;
        private const float slapDuration = 0.13f;
        private const float wrathSlapSpeed = slapSpeed * 5f;
        private const float wrathSlapDuration = slapDuration / 5f * 1.05f;

        static float minimumSlapDelay = 0.4f;
        static float decreasePerNotebook = 0.25f;
        private float extraAnger = 0f;
        private bool fireExtinguisherUsed = false;
        private bool isAngry = false;
        private bool isWrath = false;
        public static class Sprites
        {
            // Good foxo uwu
            static readonly float PIXEL_PER_UNIT = 30f;
            public static Sprite stare = AssetManager.LoadSprite("foxo/FoxoStare_0.png", PIXEL_PER_UNIT);
            public static Sprite jump = AssetManager.LoadSprite("foxo/Foxo_Jump0002.png", PIXEL_PER_UNIT);
            public static Sprite[] sprayed = new Sprite[]
            {
                AssetManager.LoadSprite("foxo/FoxoSprayed_1.png", PIXEL_PER_UNIT),
                AssetManager.LoadSprite("foxo/FoxoSprayed_2.png", PIXEL_PER_UNIT),
            };
            public static List<Sprite> wave = new List<Sprite>();
            public static Sprite[] slap = new Sprite[4] {
                AssetManager.LoadSprite("foxo/FoxoSlap_1.png", PIXEL_PER_UNIT),
                AssetManager.LoadSprite("foxo/FoxoSlap_2.png", PIXEL_PER_UNIT),
                AssetManager.LoadSprite("foxo/FoxoSlap_3.png", PIXEL_PER_UNIT),
                AssetManager.LoadSprite("foxo/FoxoSlap_4.png", PIXEL_PER_UNIT),
            };
            public static Texture2D posterTexture = AssetManager.LoadTexture("foxo/Foxo_Poster.png");

            // Wrath foxo :(
            public static Sprite[] wrath = new Sprite[3] {
                AssetManager.LoadSprite("foxo/wrath3.png", PIXEL_PER_UNIT),
                AssetManager.LoadSprite("foxo/wrath2.png", PIXEL_PER_UNIT),
                AssetManager.LoadSprite("foxo/wrath1.png", PIXEL_PER_UNIT),
            };

            // loader
            public static bool waveLoaded = false;
            public static void LoadWave()
            {
                if (waveLoaded) return;
                waveLoaded = true;
                for (int i = 0; i < 48; i++)
                    wave.Add(AssetManager.LoadSprite($"foxo/Foxo_Wave{i:0000}.png", PIXEL_PER_UNIT));
            }
        }
        public static class Sounds
        {
            public static SoundObject fear = AssetManager.LoadSoundObject("foxo/fear.wav", AudioType.WAV);
            public static SoundObject ding = AssetManager.LoadSoundObject("foxo/ding.wav", AudioType.WAV);
            public static SoundObject school = AssetManager.LoadSoundObject("foxo/school2.wav", AudioType.WAV, soundType: SoundType.Music);
            public static SoundObject hellothere = AssetManager.LoadSoundObject("foxo/hellothere.wav", AudioType.WAV, soundType: SoundType.Voice);
            public static SoundObject slap = AssetManager.LoadSoundObject("foxo/slap.wav", AudioType.WAV, "SLAP!");
            public static SoundObject slap2 = AssetManager.LoadSoundObject("foxo/slap2.wav", AudioType.WAV, "...");
            public static SoundObject scare = AssetManager.LoadSoundObject("foxo/scare.wav", AudioType.WAV);
            public static SoundObject jump = AssetManager.LoadSoundObject("foxo/boing.wav", AudioType.WAV);
            public static SoundObject scream = AssetManager.LoadSoundObject("foxo/scream.wav", AudioType.WAV);
            public static SoundObject wrath = AssetManager.LoadSoundObject("foxo/wrath.wav", AudioType.WAV);
            public static SoundObject[] praises = new SoundObject[] {
                AssetManager.LoadSoundObject("foxo/praise1.wav", AudioType.WAV),
                AssetManager.LoadSoundObject("foxo/praise2.wav", AudioType.WAV)
            };
        }
        public void Update()
        {
            PlayerManager player = players[0];
            if (!player.plm.running)
                player.plm.AddStamina(player.plm.staminaDrop * 0.8f * Time.deltaTime * player.PlayerTimeScale, true);
        }
        private IEnumerator PlayWave()
        {
            for (int i = 0; i < 48; i++)
            {
                SetSprite(Sprites.wave[i]);
                yield return new WaitForSeconds(0.05f);
            }
            SetSprite(Sprites.wave[0]);
            yield break;
        }
        private IEnumerator PlaySlap()
        {
            StopCoroutine("PlaySlap");

            if (isWrath)
            {
                audMan.PlaySingle(Sounds.slap2);
                for (int i = 0; i < 3; i++)
                {
                    SetSprite(Sprites.wrath[i]);
                    var time = 0.1f;
                    while (time > 0)
                    {
                        time -= Time.deltaTime * ec.NpcTimeScale;
                        yield return null;
                    }
                }
                yield break;
            }

            audMan.PlaySingle(Sounds.slap);
            for (int i = 0; i < 4; i++)
            {
                SetSprite(Sprites.slap[i]);
                var time = 0.1f;
                while (time > 0)
                {
                    time -= Time.deltaTime * ec.NpcTimeScale;
                    yield return null;
                }
            }
            yield break;
        }
        private IEnumerator Slap()
        {
            StartCoroutine(PlaySlap());
            navigator.SetSpeed((isWrath ? wrathSlapSpeed : slapSpeed) * ec.NpcTimeScale);
            TargetPlayer(players[0].transform.position);
            var time = isWrath ? wrathSlapDuration : slapDuration;
            while (time > 0)
            {
                time -= Time.deltaTime * ec.NpcTimeScale;
                yield return null;
            }
            navigator.SetSpeed(0f);
            navigator.maxSpeed = 0f;
            yield break;
        }
        private IEnumerator GetMad()
        {
            if (isAngry)
            {
                yield break;
            }
            isAngry = true;
            SetSprite(Sprites.stare);
            Singleton<MusicManager>.Instance.StopMidi();
            ec.audMan.FlushQueue(true);
            audMan.FlushQueue(true);
            ec.audMan.PlaySingle(Sounds.fear);
            BeginSpoopMode();

            yield return new WaitForSeconds(13f);

            ec.audMan.PlaySingle(Sounds.ding);
            SetKillable(true, Sounds.scare);
            StartCoroutine(SlapLoop());
            yield break;
        }
        private IEnumerator SlapLoop()
        {
            var time = minimumSlapDelay + decreasePerNotebook * ec.notebookTotal - Mod.Manager.gameManager.FoundNotebooks * decreasePerNotebook - extraAnger;
            if (isWrath) time *= 1.3f;
            while (time > 0)
            {
                time -= Time.deltaTime * ec.NpcTimeScale;
                yield return null;
            }
            if (!isWrath && UnityEngine.Random.Range(0, 30) == 0)
                StartCoroutine(Jump());
            else
                StartCoroutine(Slap());
            StartCoroutine(SlapLoop());
            yield break;
        }
        private IEnumerator PraisePlayer()
        {
            audMan.FlushQueue(true);
            audMan.PlaySingle(Sounds.praises.Choice());
            SetSprite(Sprites.wave[0]);
            SetKillable(false);
            navigator.ClearDestination();
            navigator.SetSpeed(0f);
            navigator.maxSpeed = 0f;
            yield return new WaitForSeconds(6);
            StartCoroutine(SlapLoop());
            yield return null;
            SetKillable(true);
            yield break;
        }
        private IEnumerator Jump()
        {
            SetSprite(Sprites.jump);
            audMan.PlaySingle(Sounds.jump);
            navigator.SetSpeed(26f * ec.NpcTimeScale);
            var time = 6f;
            while (time > 0)
            {
                time -= Time.deltaTime * ec.NpcTimeScale;
                TargetPlayer(players[0].transform.position);
                yield return null;
            }
            navigator.SetSpeed(0f);
            navigator.maxSpeed = 0f;
            SetSprite(Sprites.slap[0]);
            yield break;
        }
        public override void Awake()
        {
            base.Awake();
            CreatePoster(Sprites.posterTexture);
            Sprites.LoadWave();
            audMan.audioDevice.maxDistance *= 5;
            audMan.volumeModifier = 0.9f;
            transform.Find("SpriteBase").localPosition += new Vector3(0, -0.8f + 2.4f, 0);
            SetSprite(Sprites.wave[0]);
            Singleton<MusicManager>.Instance.StopMidi();
            audMan.PlaySingle(Sounds.hellothere);
            ec.audMan.PlaySingle(Sounds.school);
            DontSpawnNPCS = false;
            StartCoroutine(PlayWave());
        }
        private IEnumerator FireExtinguisherSequence()
        {
            audMan.FlushQueue(true);
            audMan.PlaySingle(Sounds.scream);
            SetKillable(false);
            var time = 16f;
            var flag = false;
            while (time > 0)
            {
                time -= Time.deltaTime;
                flag = !flag;
                SetSprite(flag ? Sprites.sprayed[0] : Sprites.sprayed[1]);
                yield return null;
            }
            SetKillable(true, Sounds.scare);
            StartCoroutine(SlapLoop());
            yield break;
        }
        private void FireExtinguisher()
        {
            if (fireExtinguisherUsed) return;
            StopAllCoroutines();
            StartCoroutine(FireExtinguisherSequence());
        }
        public void TriggerWrath()
        {
            if (!isWrath)
            {
                isWrath = true;
                ec.audMan.PlaySingle(Sounds.wrath);
                foreach (NPC npc in ec.npcs.ToList())
                {
                    if (npc != this)
                    {
                        npc.Despawn();
                    }
                }
            }
        }
        public override void NotebookCollected()
        {
            base.NotebookCollected();
            if (Singleton<CoreGameManager>.Instance.currentMode == Mode.Free)
                return;
            if (!isAngry)
            {
                StopAllCoroutines();
                StartCoroutine(GetMad());
                var (door, _) = TeleportToNearestDoor();

                navigator.SetSpeed(100f);
                navigator.maxSpeed = 100f;

                TargetPosition(door.transform.position);
            }
            if (ec.notebookTotal >= 8 && Mod.Manager.gameManager.foundNotebooks >= ec.notebookTotal / 2)
                TriggerWrath();
        }
        public override void WrongMathMachineAnswer()
        {
            extraAnger += 0.0075f;
        }
        public override void GoodMathMachineAnswer()
        {
            if (!isAngry) return;
            StopAllCoroutines();
            StartCoroutine(PraisePlayer());
        }
        public override void PlayerExitedSpawn()
        {
        }
        public override void ChalkEraserUsed(Vector3 position)
        {
            if (Vector3.Distance(position, transform.position) <= 60f)
                FireExtinguisher();
        }
    }
}
