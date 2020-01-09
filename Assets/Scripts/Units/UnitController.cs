using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MKG.MB_NC
{
    public abstract class UnitController : MonoBehaviour
    {

        protected UnitManager _unitManager;
        protected UnitMovement _unitMovement;
        protected AudioSource _click;
        protected HexGrid _grid;
        protected Animator _animator;


        public abstract void OnLeftMouseDownMovement();
        public abstract void OnLeftMouseDownFight();
        public abstract void OnRightMouseDownMovement();
        public abstract void OnRightMouseDownFight();
        public abstract void OnLeftMouseDownEnemyFightCheck();
        public abstract void OnLeftMouseDownEnemyFightUncheck();
        public abstract void OnRightMouseDownEnemyFight();


        public virtual void Setup(UnitManager unitManager, UnitMovement unitMovement)
        {
            _unitManager = unitManager;
            _grid = _unitManager.Grid;
            _unitMovement = unitMovement;
            _animator = GetComponent<Animator>();
            _click = GameObject.Find("Click Source").GetComponent<AudioSource>();
        }

        public virtual void HandleLeftClick(Hex hex)
        {
            Debug.Log("OOOO");
            if (_unitManager.CurrentState == UnitManager.State.Idle)
            {
                _click.Play();
                if (_grid.ShowPath && hex == _grid.Path[_grid.Path.Count - 1]) _unitMovement.GoToTarget();
                else _unitMovement.FindPath(hex);
            }
            else if (_unitManager.CurrentState == UnitManager.State.Moving || _unitManager.CurrentState == UnitManager.State.Rotating) _unitMovement.RewindMovement();
        }

        public void HandleRightClick(Hex hex)
        {
            Debug.Log("Held!");
        }
    }
}
