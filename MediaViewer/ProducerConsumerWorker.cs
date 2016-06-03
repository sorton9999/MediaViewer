using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer
{
    public class WorkItem
    {
        public int Id { get; set; }
        public Object Item { get; set; }

        public WorkItem(int _id, Object _item)
        {
            Id = _id;
            Item = _item;
        }
    }

    public delegate void WorkDelegate(Object workItem);

    public class ProducerConsumerWorker
    {
        public int workLength = 0;
        private bool enqueuOnce = false;
        public static event WorkDelegate workEvent;

        private ManualResetEvent resetEvent;

        private bool workComplete = false;

        public int WorkLength
        {
            get;
            protected set;
        }

        public bool EnqueueOnce
        {
            get { return enqueuOnce; }
            protected set { enqueuOnce = value; }
        }

        public void Produce(WorkItem item)
        {
            ThreadPool.QueueUserWorkItem
                (
                    new WaitCallback(Consume), item
                );
            workLength++;
        }

        protected void Consume(Object obj)
        {
            WorkItem item = obj as WorkItem;
            if (obj != null)
            {
                WorkDelegate localEvent = workEvent;
                if (localEvent != null)
                {
                    localEvent(item.Item);
                    lock (this)
                    {
                        workLength--;
                        if (!workComplete)
                        {
                            return;
                        }
                    }
                    if (workLength == 0)
                    {
                        resetEvent.Set();
                        enqueuOnce = true;
                    }
                }
            }
        }

        public void Wait()
        {
            lock (this)
            {
                if (workLength == 0)
                {
                    return;
                }
                resetEvent = new ManualResetEvent(false);
                workComplete = true;
            }
            resetEvent.WaitOne();
        }

    }
}
