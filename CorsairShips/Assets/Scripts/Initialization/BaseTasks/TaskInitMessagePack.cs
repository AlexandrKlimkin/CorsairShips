using PestelLib.TaskQueueLib;

namespace Game.Initialization.Base {
    public class TaskInitMessagePack : Task {

        public override void Run() {
#if ENABLE_IL2CPP
            CompositeResolver.RegisterAndSetAsDefault(
                MessagePack.Resolvers.GeneratedResolverNormal.Instance, 
                GeneratedResolverPlugins.Instance,

                // finally, use builtin/primitive resolver(don't use StandardResolver, it includes dynamic generation)
                BuiltinResolver.Instance,
                AttributeFormatterResolver.Instance,
                PrimitiveObjectResolver.Instance,
                UnityResolver.Instance
            );
#endif
            OnComplete(this);
        }
    }
}
