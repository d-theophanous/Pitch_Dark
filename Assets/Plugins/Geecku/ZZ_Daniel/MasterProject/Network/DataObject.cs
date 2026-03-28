using Geecku.DefaultNetworking.Attachables;
using UnityEngine;

namespace Daniel.Master
{
    public class DataObject : DataObject<NetworkTypes, SyncObject>
    {
        protected override SyncObject CreateSyncObjectFromHash(HashObject<NetworkTypes> hash)
        {
            return null;
        }
        protected override void DeleteSyncObject(SyncObject sync_object)
        {

        }
    }
}
