using System.Threading.Tasks;
using TimedDictionary.AfterTaskCompletionBehaviours;

namespace TimedDictionary
{
    public abstract class AfterTaskCompletion
    {
        public static readonly AfterTaskCompletion DoNothing = new AfterTaskCompletion_DoNothing();
        public static readonly AfterTaskCompletion RemoveFromDictionary = new AfterTaskCompletion_RemoveFromDictionary();


        internal abstract void Handle<Key, Value>(DictionaryEntry<Key, Value> entry) where Value : Task;
    }
}