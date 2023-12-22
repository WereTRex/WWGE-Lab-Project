using UnityEngine;
using UnityHFSM;

namespace EnemyStates.Standard
{
    public class Dead : State
    {
        public override string Name { get => "Dead"; }


        private readonly GameObject _enemyGO;
        private readonly float _destroyTime;

        public Dead(GameObject enemyObject, float destroyTime)
        {
            this._enemyGO = enemyObject;
            this._destroyTime = destroyTime;
        }


        public override void OnEnter() => GameObject.Destroy(this._enemyGO, _destroyTime);
        
    }
}