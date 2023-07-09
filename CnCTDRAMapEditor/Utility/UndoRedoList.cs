//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.

// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using System;
using System.Collections.Generic;

namespace MobiusEditor.Utility
{
    public interface IUndoRedoEventArgs<T>
    {
        /// <summary>
        /// Allows giving a warning for large undo/redo events, and cancelling if the warning makes the user reconsider the action.
        /// </summary>
        bool Cancelled { get; set; }

        /// <summary>
        /// Source of the change
        /// </summary>
        T Source { get; set; }
    }

    public class UndoRedoList<T, U> where T: EventArgs, IUndoRedoEventArgs<U>
    {
        private const int DefaultMaxUndoRedo = 50;

        private readonly List<(Action<T> Undo, Action<T> Redo, U Source)> undoRedoActions = new List<(Action<T> Undo, Action<T> Redo, U Source)>();
        private readonly int maxUndoRedo;
        private int undoRedoPosition = 0;

        public event EventHandler<EventArgs> Tracked;
        public event EventHandler<T> Undone;
        public event EventHandler<T> Redone;

        public bool CanUndo => undoRedoPosition > 0;

        public bool CanRedo => undoRedoActions.Count > undoRedoPosition;

        public UndoRedoList(int maxUndoRedo)
        {
            this.maxUndoRedo = maxUndoRedo;
        }

        public UndoRedoList()
            : this(DefaultMaxUndoRedo)
        {
        }

        public void Clear()
        {
            undoRedoActions.Clear();
            undoRedoPosition = 0;
            OnTracked();
        }

        /// <summary>
        /// Adds an item to the undo/redo list.
        /// </summary>
        /// <param name="undo">The action to perform to undo the executed operation.</param>
        /// <param name="redo">The action to perform to redo the executed operation.</param>
        /// <param name="source">The source that performed the original action.</param>
        public void Track(Action<T> undo, Action<T> redo, U source)
        {
            if (undoRedoActions.Count > undoRedoPosition)
            {
                undoRedoActions.RemoveRange(undoRedoPosition, undoRedoActions.Count - undoRedoPosition);
            }
            undoRedoActions.Add((undo, redo, source));
            if (undoRedoActions.Count > maxUndoRedo)
            {
                undoRedoActions.RemoveRange(0, undoRedoActions.Count - maxUndoRedo);
            }
            undoRedoPosition = undoRedoActions.Count;
            OnTracked();
        }

        /// <summary>
        /// Undo the current action in the undo/redo list.
        /// </summary>
        /// <param name="context">Context event argument. The <typeparamref name="U"/> <paramref name="context.Source "/> will be filled in automatically.</param>
        /// <exception cref="InvalidOperationException">There are no undo actions left in the list. Check <see cref="CanUndo"/> to avoid getting this.</exception>
        public void Undo(T context)
        {
            if (!CanUndo)
            {
                throw new InvalidOperationException();
            }
            undoRedoPosition--;
            // Set source into event.
            context.Source = undoRedoActions[undoRedoPosition].Source;
            undoRedoActions[undoRedoPosition].Undo(context);
            if (!context.Cancelled)
            {
                OnUndone(context);
            }
            else
            {
                undoRedoPosition++;
            }
        }

        /// <summary>
        /// Redo the current action in the undo/redo list.
        /// </summary>
        /// <param name="context">Context event argument. The <typeparamref name="U"/> <paramref name="context.Source "/> will be filled in automatically.</param>
        /// <exception cref="InvalidOperationException">There are no redo actions left in the list. Check <see cref="CanRedo"/> to avoid getting this.</exception>
        public void Redo(T context)
        {
            if (!CanRedo)
            {
                throw new InvalidOperationException();
            }
            // Set source into event.
            context.Source = undoRedoActions[undoRedoPosition].Source;
            undoRedoActions[undoRedoPosition].Redo(context);
            if (!context.Cancelled)
            {
                undoRedoPosition++;
                OnRedone(context);
            }
        }

        protected virtual void OnTracked()
        {
            Tracked?.Invoke(this, new EventArgs());
        }

        protected virtual void OnUndone(T context)
        {
            Undone?.Invoke(this, context);
        }

        protected virtual void OnRedone(T context)
        {
            Redone?.Invoke(this, context);
        }
    }
}
