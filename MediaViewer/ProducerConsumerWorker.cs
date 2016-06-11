using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer
{
    /// <summary>
    /// A class that is the object passed to the threading producer in
    /// the MediaPlayProducerConsumer class.
    /// </summary>
    public class WorkItem
    {
        /// <summary>
        /// The Id value of the passed item
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The item being passed
        /// </summary>
        public Object Item { get; set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_id">The Id value</param>
        /// <param name="_item">The passed item</param>
        public WorkItem(int _id, Object _item)
        {
            Id = _id;
            Item = _item;
        }
    }

    /// <summary>
    /// The thread work delegate
    /// </summary>
    /// <param name="workItem">The item to do work on</param>
    public delegate void WorkDelegate(Object workItem);

    /// <summary>
    /// The Producer/Consumer base class which provides a threading capability.
    /// The Producer queues up threads on a pool with Consume being called on
    /// each thread.  The Consume handler calls the WorkEvent.
    /// </summary>
    public class ProducerConsumerWorker
    {
        /// <summary>
        /// The length of the work queue
        /// </summary>
        public int workLength = 0;
        
        /// <summary>
        /// Have all the threads in the queue been called
        /// </summary>
        private bool workComplete = false;

        /// <summary>
        /// Has at least one work thread been called
        /// </summary>
        private bool enqueuOnce = false;

        /// <summary>
        /// A static Work Event called by the Consume method
        /// </summary>
        public static event WorkDelegate workEvent;

        /// <summary>
        /// A manual reset event object
        /// </summary>
        private ManualResetEvent resetEvent;

        /// <summary>
        /// Public property for the queue length
        /// </summary>
        public int WorkLength
        {
            get;
            protected set;
        }

        /// <summary>
        /// Public property for the enqueue flag
        /// </summary>
        public bool EnqueueOnce
        {
            get { return enqueuOnce; }
            protected set { enqueuOnce = value; }
        }

        /// <summary>
        /// The public producer call to start a thread on the queue
        /// </summary>
        /// <param name="item">The work item to pass in</param>
        public void Produce(WorkItem item)
        {
            ThreadPool.QueueUserWorkItem
                (
                    new WaitCallback(Consume), item
                );
            workLength++;
        }

        /// <summary>
        /// The consumer call that will call the work event and pass in the
        /// work item.
        /// </summary>
        /// <param name="obj">The work item</param>
        protected void Consume(Object obj)
        {
            WorkItem item = obj as WorkItem;
            if (obj != null)
            {
                WorkDelegate localEvent = workEvent;
                if (localEvent != null)
                {
                    // Call the work event
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
                        // Reset the manual reset object when all work is complete
                        if (resetEvent != null)
                        {
                            resetEvent.Set();
                        }
                        // At least one work event has been completed
                        enqueuOnce = true;
                    }
                }
            }
        }

        /// <summary>
        /// A wait call to allow the called threads to complete.  A wait
        /// is called on the manual reset object which is then reset
        /// in the Consume method when all work is done.
        /// </summary>
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
