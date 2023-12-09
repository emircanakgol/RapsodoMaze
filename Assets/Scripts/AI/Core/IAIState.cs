
namespace AI.Core
{
    public interface IAIState
    {
        public void Enter();
        public void Update();
        public void Exit();
    }
}