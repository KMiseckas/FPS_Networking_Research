using UnityEngine;

namespace WorkingTitle
{
    public class Player : Entity
    {
        #region Unity Messages

        protected override void Update()
        {
            this.Rotate(new Vector2()
            {
                x = Input.GetAxis("Mouse Y"),
                y = Input.GetAxis("Mouse X")
            });
            this.Move(new Vector3()
            {
                x = Input.GetAxis("Horizontal"),
                y = 0,
                z = Input.GetAxis("Vertical")
            });

            this.IsSprinting = Input.GetKey(KeyCode.LeftShift);

            base.Update();
        }

        #endregion
    }
}