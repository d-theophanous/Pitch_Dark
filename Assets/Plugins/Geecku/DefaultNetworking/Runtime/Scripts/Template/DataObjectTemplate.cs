using Geecku.DefaultNetworking.Attachables;
using UnityEngine;

namespace Geecku.DefaultNetworking.Template
{
    public class DataObjectTemplate : DataObject<NetworkTypes, SyncObjectTemplate>
    {
        protected override SyncObjectTemplate CreateSyncObjectFromHash(HashObject<NetworkTypes> hash)
        {
            return null;
        }
        protected override void DeleteSyncObject(SyncObjectTemplate sync_object)
        {

        }
    }
}
