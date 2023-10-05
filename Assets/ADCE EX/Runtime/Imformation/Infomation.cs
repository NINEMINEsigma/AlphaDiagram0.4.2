using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    [Serializable]
    public sealed class TaskInfo : IComparable<TaskInfo>
    {
        public string TaskName;
        public int TaskIndex;
        private float _TaskPercent;
        public float TaskPercent
        {
            get => _TaskPercent;
            set
            {
                _TaskPercent = Mathf.Clamp(value, 0, 1.01f);
            }
        }
        public Vector2 Range = new Vector2(0, 1);
        public bool IsInt;

        public void Wait()
        {
            GameEditorApp.instance.GetModel<TaskList>().Wait(this);
        }

        public void WaitLast()
        {
            GameEditorApp.instance.GetModel<TaskList>().WaitLast(this);
        }

        public int CompareTo(TaskInfo other)
        {
            return this.TaskIndex.CompareTo(other.TaskIndex);
        }

        public void Register()
        {
            GameEditorApp.instance.GetModel<TaskList>().AddTask(this);
        }

        public void UnRegister()
        {
            GameEditorApp.instance.GetModel<TaskList>().RemoveTask(this);
        }
    }

    public sealed class TaskList : ADModel
    {
        public override void Init()
        {
            Current = null;
            Tasks.Clear();
            AddTaskCallBack = new();
            RemoveTaskCallBack = new();
            CompleteTaskCallBack = new();
        }

        public TaskInfo Current { get; private set; }
        public List<TaskInfo> Tasks { get; private set; } = new();

        public override IADModel Load(string path)
        {
            throw new NotImplementedException();
        }

        public override void Save(string path)
        {
            throw new NotImplementedException();
        }

        public ADEvent<TaskInfo> AddTaskCallBack = new();
        public ADEvent<TaskInfo> RemoveTaskCallBack = new();
        public ADEvent<TaskInfo> CompleteTaskCallBack = new();

        public void AddTask(TaskInfo task)
        {
            if (Current == null)
            {
                Current = task;
            }
            else
            {
                Tasks.Add(task);
                Tasks.Sort();
            }
            AddTaskCallBack.Invoke(task);
        }

        public void RemoveTask(TaskInfo task)
        {
            if (Current == task)
            {
                if (Tasks.Count > 0)
                    Current = Tasks[0];
                else Current = null;
            }
            else
            {
                Tasks.Add(task);
                Tasks.Sort();
            }
            RemoveTaskCallBack.Invoke(task);
        }

        public void Update()
        {
            if (Current.TaskPercent >= 1)
            {
                CompleteTaskCallBack.Invoke(Current);
                RemoveTask(Current);
                Update();
            }
        }

        public void Wait(TaskInfo task)
        {
            if (Current == task)
            {
                if (Current == task && Tasks.Count == 0) return;
                else
                {
                    Current = Tasks[0];
                    Tasks[0] = task;
                }
            }
            else if (Tasks.Count > 1 && Tasks[^1] != task)
            {
                int targetIndex = Tasks.FindIndex(T => T == task);
                var temp = Tasks[targetIndex + 1];
                Tasks[targetIndex + 1] = task;
                Tasks[targetIndex] = temp;
            }
            Update();
        }

        public void WaitLast(TaskInfo task)
        {
            if (Current == task)
            {
                if (Current == task && Tasks.Count == 0) return;
                else
                {
                    Current = Tasks[^1];
                    Tasks[^1] = task;
                }
            }
            else if (Tasks.Count > 1 && Tasks[^1] != task)
            {
                int targetIndex = Tasks.FindIndex(T => T == task);
                var temp = Tasks[^1];
                Tasks[^1] = task;
                Tasks[targetIndex] = temp;
            }
            Update();
        }
    }

    public class Infomation : ADController
    {
        [SerializeField] AD.UI.Text LeftText;
        [SerializeField] AD.UI.Text RightText;
        [SerializeField] Animator TaskPanelAnimator;
        [SerializeField] AD.UI.Text TaskPanelTitle;
        [SerializeField] AD.UI.ListView TaskPanelListView;
        [SerializeField] AD.UI.ModernUIFillBar TaskPanelPercentBar;
        [SerializeField] TaskViewItem TaskViewItemPerfab;

        private void Start()
        {
            GameEditorApp.instance.RegisterController(this);
        }

        public override void Init()
        {
            TaskList _m_TaskList = new();
            Architecture.UnRegister<TaskList>();
            Architecture.RegisterModel(_m_TaskList);
            _m_TaskList.AddTaskCallBack.AddListener(T => Refresh());
            _m_TaskList.RemoveTaskCallBack.AddListener(T => Refresh());
            _m_TaskList.CompleteTaskCallBack.AddListener(T => Refresh());
            _m_TaskList.CompleteTaskCallBack.AddListener(CompleteTask);
            TaskPanelListView.SetPrefab(TaskViewItemPerfab);
        }

        bool IsOpenTaskPanel = false;
        public void ClickTaskProgramBar()
        {
            IsOpenTaskPanel = !IsOpenTaskPanel;
            TaskPanelAnimator.Play(IsOpenTaskPanel ? "Open" : "Hide");
        }

        public void Log(string message)
        {
            SetLeft(message);
        }

        public void Warning(string message)
        {
            SetLeft(message);
            LeftText.source.color = Color.yellow;
        }

        public void Error(string message)
        {
            SetLeft(message);
            LeftText.source.color = Color.red;
        }

        public void Version(string version)
        {
            SetRight(version);
        }

        public void SetLeft(string text)
        {
            LeftText.SetText(text);
        }

        public void SetRight(string text)
        {
            RightText.SetText(text);
        }

        private TaskViewItem RegisterTaskListItem(TaskInfo task,int index)
        {
            var item = TaskPanelListView.GenerateItem().As<TaskViewItem>();
            item.taskInfo = task;
            item.Refresh();
            task.TaskIndex = index;
            return item;
        }

        public void Refresh()
        {
            var _m_TaskList = Architecture.GetModel<TaskList>();
            _m_TaskList.Update();
            if (_m_TaskList.Current == null)
            {
                TaskPanelTitle.SetText("");
                TaskPanelListView.Clear();
                TaskPanelPercentBar.Init();
            }
            else
            {
                var current = _m_TaskList.Current;
                TaskPanelPercentBar.Set(current.TaskPercent, current.Range.x, current.Range.y);
            }
            TaskPanelListView.Clear();
            RegisterTaskListItem(_m_TaskList.Current,0);
            for (int i = 0; i < _m_TaskList.Tasks.Count; i++)
            {
                TaskInfo task = _m_TaskList.Tasks[i];
                RegisterTaskListItem(task, i + 1);
            }
            TaskPanelListView.SortChilds();
        }

        private void CompleteTask(TaskInfo task)
        {
            Architecture.AddMessage(task.TaskName + " is complete");
        }

    }
}
