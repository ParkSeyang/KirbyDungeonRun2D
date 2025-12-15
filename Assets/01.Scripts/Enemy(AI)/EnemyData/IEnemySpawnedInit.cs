namespace EnemySystem.Runtime
{
    // EnemySpawnedContext가 InitializeFromEntry() 끝난 직후 호출해주는 훅
    public interface IEnemySpawnedInit
    {
        void OnSpawned(EnemySpawnedContext ctx);
    }
    
}