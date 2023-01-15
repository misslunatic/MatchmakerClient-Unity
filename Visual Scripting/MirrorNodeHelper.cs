using Mirror;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Scripts.Matchmaker
{
    public class MirrorNodeHelper : NetworkBehaviour
    {
        [Client]
        public GameObject SpawnObject(GameObject iG, Vector3 iPos, Quaternion iRot, Transform iParent, Vector3 iScale)
        {

            SpawnObjectCmd(NetworkManager.singleton.spawnPrefabs.IndexOf(iG), iPos, iRot, iParent, iScale);
            return GameObject.FindGameObjectWithTag("MirNodeHelperSpawned");
        }
        
        [Command(requiresAuthority = false)]
        void SpawnObjectCmd(int iG, Vector3 iPos, Quaternion iRot, Transform iParent, Vector3 iScale)
        {
            GameObject go;
            Debug.Log("Creating object...");
            if (iParent != null)
            { 
                go = Instantiate(NetworkManager.singleton.spawnPrefabs[iG], iPos, iRot, iParent);
            }
            else
            {
                go = Instantiate(NetworkManager.singleton.spawnPrefabs[iG], iPos, iRot, iParent);
            }

            go.transform.localScale = iScale;
            go.gameObject.tag = "MirNodeHelperSpawned";
            NetworkServer.Spawn(go);
            
        }
    }
}