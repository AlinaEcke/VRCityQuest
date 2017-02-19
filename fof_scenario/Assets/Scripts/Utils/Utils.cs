using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FoF.Utils
{
    [Serializable]
    public class ReceiverItem
    {
        public GameObject receiver;
        public string action = "OnSignal";
        public float delay = 0.0f;

        public IEnumerator SendWithDelay(MonoBehaviour sender)
        {
            yield return new WaitForSeconds(delay);

            if (receiver != null)
                receiver.SendMessage(action);
            else
                Debug.LogWarning("No receiver of signal \"" + action + "\" on object " + sender.name + " (" + sender.GetType().Name + ")", sender);
        }

        public IEnumerator SendWithDelay(MonoBehaviour sender, object param)
        {
            yield return new WaitForSeconds(delay);

            if (receiver != null)
                receiver.SendMessage(action, param);
            else
                Debug.LogWarning("No receiver of signal \"" + action + "\" on object " + sender.name + " (" + sender.GetType().Name + ")", sender);
        }
    }

    [Serializable]
    public class SignalSender
    {
        public bool onlyOnce;
        public List<ReceiverItem> receiversList;

        protected bool hasFired = false;

        public void Register(GameObject receiver, string action, float delay)
        {
            this.Unregister(receiver, action);

            ReceiverItem item = new ReceiverItem();
            item.receiver = receiver;
            item.action = action;
            item.delay = delay;
            receiversList.Add(item);
        }

        public void Unregister(GameObject receiver, string action)
        {
            for (int i = receiversList.Count - 1; i >= 0; --i)
            {
                if (receiversList[i].receiver == receiver && receiversList[i].action == action)
                    receiversList.RemoveAt(i);
            }
        }

        public void SendSignal(MonoBehaviour sender)
        {
            if (!hasFired || !onlyOnce)
            {
                for (int i = 0; i < receiversList.Count; i++)
                    sender.StartCoroutine(receiversList[i].SendWithDelay(sender));
            }
            hasFired = true;
        }

        public void SendSignal(MonoBehaviour sender, object param)
        {
            if (!hasFired || !onlyOnce)
            {
                for (int i = 0; i < receiversList.Count; i++)
                    sender.StartCoroutine(receiversList[i].SendWithDelay(sender, param));
            }
            hasFired = true;
        }
    }

    [Serializable]
    public class StringPerc
    {
        public string item;
        public int perc;
    }

    public enum DistributionType
    {
        Street = 0,
        City
    }

    public enum AssetLocation
    {
        Ground = 0,
        Floor
    }
}