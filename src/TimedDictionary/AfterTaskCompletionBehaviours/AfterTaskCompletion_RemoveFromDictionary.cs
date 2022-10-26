using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimedDictionary.AfterTaskCompletionBehaviours
{
    public class AfterTaskCompletion_RemoveFromDictionary : AfterTaskCompletion
    {
        internal override void Handle<Key, Value>(DictionaryEntry<Key, Value> entry)
        {
            entry.Value.ContinueWith((state) => 
            {   
                entry.RemoveItselfFromDictionary();
            });
        }
    }
}