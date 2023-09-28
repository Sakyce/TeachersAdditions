using System.Collections;
using System.Collections.Generic;
using Additions;
using Additions.Characters;
using HarmonyLib;
using UnityEngine;
using Utility;

namespace TeachersAdditions.Patches
{
    [HarmonyPatch(typeof(OfficeBuilderStandard), nameof(OfficeBuilderStandard.Builder))]
    internal class ReplaceBaldiPosterPatch
    {
        static IEnumerator Builder(OfficeBuilderStandard office)
        {
            while (!office.lg.DoorsFinished)
            {
                yield return null;
            }
            List<TileShape> shapes = new List<TileShape>
            {
                TileShape.Single,
                TileShape.Corner
            };
            List<TileController> tilesOfShape = office.room.GetTilesOfShape(shapes, true);
            List<NPC> list = new List<NPC>(office.lg.Ec.npcsToSpawn);
            int num = 0;
            while (num < 5 && list.Count > 0 && tilesOfShape.Count > 0)
            {
                int index = office.cRNG.Next(0, tilesOfShape.Count);
                int index2 = office.cRNG.Next(0, list.Count);
                
                var npc = list[index2];
                if (npc is Baldi)
                {
                    var tileToBuildPoster = tilesOfShape[index];
                    var randomDir = tilesOfShape[index].RandomFreeDirection(office.cRNG);
                    Mod.instance.BuildPosterAtAndWaitForTeacher(tileToBuildPoster, randomDir);
                }
                else
                {
                    office.room.ec.BuildPoster(npc.Poster, tilesOfShape[index], tilesOfShape[index].RandomFreeDirection(office.cRNG));
                }
               
                tilesOfShape.RemoveAt(index);
                list.RemoveAt(index2);
                num++;
            }
            foreach (TileController tileController in tilesOfShape)
            {
                if (office.cRNG.NextDouble() * 100.0 < (double)office.windowChance)
                {
                    office.lg.Ec.BuildWindow(tileController, tileController.wallDirections[office.cRNG.Next(0, tileController.wallDirections.Length)], office.windowObject);
                }
            }
            office.deskPlacer.Build(office.lg.Ec, office.room, office.cRNG);
            office.tapePlayer.transform.position = office.deskPlacer.ObjectsPlaced[0].transform.position + Vector3.up * 5f;
            office.deskSpawner.SetRange(office.lg.Ec.RealRoomMin(office.room), office.lg.Ec.RealRoomMax(office.room));
            office.deskSpawner.StartSpawner(office.lg.Ec, new System.Random(office.cRNG.Next()));
            office.room.AddItemSpawns(office.deskSpawner.ObjectsSpawned);
            while (!office.deskSpawner.Finished)
            {
                yield return null;
            }
            office.building = false;
            yield break;
        }
        static void Postfix(OfficeBuilderStandard __instance, ref IEnumerator __result)
        {
            __result = new SimpleEnumerator() { enumerator = Builder(__instance) }.GetEnumerator();
        }
    }
}
