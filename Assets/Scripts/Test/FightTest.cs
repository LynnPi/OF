using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FightTest : MonoBehaviour {

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    void OnGUI() {
        if( GUI.Button( new Rect( 0, 0, 100, 100 ), "开始战斗" ) ) {
            List<ClientShip> playerShipList = new List<ClientShip>();
            List<ClientShip> enemyShipList = new List<ClientShip>();

            // player
            ClientShip ship = new ClientShip();
            ship.ReadFromId( 10000 );
            ship.Position = new Vector3( 100, 0, 0 );
            playerShipList.Add( ship );

            ship = new ClientShip();
            ship.ReadFromId( 10001 );
            ship.Position = new Vector3( 300, 0, 0 );
            playerShipList.Add( ship );

            // enemy
            ship = new ClientShip();
            ship.ReadFromId( 10000 );
            ship.Position = new Vector3( 100, 0, 400 );
            enemyShipList.Add( ship );

            ship = new ClientShip();
            ship.ReadFromId( 10001 );
            ship.Position = new Vector3(300, 0, 400);
            enemyShipList.Add( ship );

            FightService.Instance.InitService( playerShipList, enemyShipList );
            FightService.Instance.BeginFight();
        }
    }
}
