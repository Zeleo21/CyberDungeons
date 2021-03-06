using System.Collections.Generic;
using AI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = System.Random;

namespace Map
{
    public class TriggerEnemyArea : MonoBehaviour
    {
        private const int distanceToSpawn = 15;
        private const int delta = 12;

        public List<GameObject> mobs;
        public Transform Spawner;
        public bool hasSpawned;

        public static int aliveMob;

        private void Awake()
        {
            DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;

            foreach (GameObject prefab in mobs)
            {
                if (!pool.ResourceCache.ContainsKey(prefab.name))
                    pool.ResourceCache.Add(prefab.name, prefab);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //On vérifie si les ennemis n'ont pas déjâ spawn et on éxécute uniquement avec le master client
            if (!hasSpawned && PhotonNetwork.IsMasterClient)
            {
                Vector3 position = Spawner.position;
                Random random = new Random();
                int a = 0;
                int b = 0;
                
                switch (MapGenerator.level)
                {
                    case 1:
                        a = 2;
                        b = 5;
                        break;
                    case 2:
                        a = 3;
                        b = 6;
                        break;
                    case 3:
                        a = 4;
                        b = 7;
                        break;
                }
                
                int hasToSpawn = random.Next(a, b);
                aliveMob = hasToSpawn;
                
                //On fait apparaître les murs
                Map map = Map.FindMapByVector(position);
                map.SpawnWall();

                //On téléporte les joueurs comme expliqué avant
                if (PhotonNetwork.MasterClient.NickName == other.name)
                {
                    gameObject.GetComponent<PhotonView>()
                        .RPC("TPPlayer", RpcTarget.Others, other.transform.position, other.name);
                }

                //Tant que tout les monstres n'ont pas spawn, on cherche une position
                while (hasToSpawn != 0)
                {
                    float x = position.x +
                              random.Next((-MapGenerator.sizeX + delta) / 2, (MapGenerator.sizeX - delta) / 2);
                    float y = position.y +
                              random.Next((-MapGenerator.sizeY + delta) / 2, (MapGenerator.sizeY - delta) / 2);

                    bool ok = true;
                    Vector2 transformPosition = new Vector2(x, y);
                    
                    //On vérifie si la distance est assez loin des joueurs
                    foreach (GameObject player in PlayerConnect.players)
                    {
                        if (player == null)
                            continue;
                        if (Vector3.Distance(player.transform.position, transformPosition) < distanceToSpawn)
                            ok = false;
                    }

                    //On regarde si l'ennemi n'est pas dans un mur
                    if (Physics2D.Linecast(transformPosition, transformPosition,
                        1 << LayerMask.NameToLayer("WallColider")))
                    {
                        ok = false;
                    }

                    //On fait apparaître l'ennemi s'il les conditions sont remplis
                    if (ok)
                    {
                        PhotonNetwork.Instantiate(mobs[random.Next(mobs.Count)].name, transformPosition,
                            Quaternion.identity);
                        Debug.Log("Mob spawn in x:" + x + "  y:" + y);
                        gameObject.GetComponent<BoxCollider2D>().enabled = false;
                        hasSpawned = true;
                        hasToSpawn--;
                    }
                }
            }

            if (PhotonNetwork.MasterClient.NickName != other.name)
            {
                GameObject obj = GameObject.Find(PhotonNetwork.MasterClient.NickName);
                if (obj != null)
                    obj.transform.position = other.transform.position;
            }
        }

        [PunRPC]
        public void TPPlayer(Vector3 pos, string name)
        {
            foreach (Player pl in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (pl.NickName == name)
                    continue;

                GameObject find = GameObject.Find(pl.NickName);
                if (find != null)
                    find.transform.position = pos;
            }
        }
    }
}