using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace Shurub
{
    public class ZoneManager : SingletonPun<ZoneManager>
    {
        private readonly List<IngredientZone> zones = new();

        public void Register(IngredientZone zone)
        {
            if (!zones.Contains(zone))
                zones.Add(zone);
        }

        public void ResetAllZones()
        {
            photonView.RPC(
                nameof(RPC_ResetAllZones),
                RpcTarget.MasterClient
            );
        }
        [PunRPC]
        public void RPC_ResetAllZones()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            foreach (var zone in zones)
            {
                zone.ResetZone();
            }
        }
    }
}