using System.Collections.Generic;
using Core;

namespace Gameplay
{
    public static class EnemySaveManager
    {
        //serialize enemies to manage respawns
        public static List<EnemySaveData> GetEnemyStates()
        {
            var enemyStates = new List<EnemySaveData>();

            foreach (var enemy in EnemySaveTarget.Enemies)
            {
                enemyStates.Add(new EnemySaveData
                {
                    id = enemy.EnemyId,
                    isAlive = enemy.IsAlive
                });
            }

            return enemyStates;
        }

        //restore every enemies with original state if dead and player didn't save
        public static void RestoreEnemyStates(List<EnemySaveData> enemyStates)
        {
            foreach (var enemy in EnemySaveTarget.Enemies)
            {
                foreach (var enemyState in enemyStates)
                {
                    if (enemy.EnemyId != enemyState.id)
                        continue;

                    enemy.RestoreState(enemyState);
                    break;
                }
            }
        }
    }
}