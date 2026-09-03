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
        public static List<EnemySaveData> MergeEnemyStates(List<EnemySaveData> savedEnemyStates)
        {
            var enemyStates = savedEnemyStates != null
                ? new List<EnemySaveData>(savedEnemyStates)
                : new List<EnemySaveData>();

            foreach (var currentEnemyState in GetEnemyStates())
            {
                int savedStateIndex = enemyStates.FindIndex(enemyState => enemyState.id == currentEnemyState.id);

                if (savedStateIndex >= 0)
                    enemyStates[savedStateIndex] = currentEnemyState;
                else
                    enemyStates.Add(currentEnemyState);
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
        public static void RestoreEnemyStates(List<EnemySaveData> enemyStates, string sceneName)
        {
            foreach (var enemy in EnemySaveTarget.Enemies)
            {
                if (enemy.gameObject.scene.name != sceneName)
                    continue;

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