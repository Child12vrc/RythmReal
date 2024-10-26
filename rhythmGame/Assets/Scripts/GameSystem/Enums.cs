using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGame
{
    public class Enums
    {

        /// <summary>
        /// 값을 변화할때 사용함
        /// </summary>
        public enum Modifier
        {
            /// <summary>
            /// 없음
            /// </summary>
            NONE,
            /// <summary>
            /// 긍정, 양수
            /// </summary>
            POSITIVE,
            /// <summary>
            /// 부정, 음수
            /// </summary>
            NEGATIVE

        }
        public enum NoteType
        {
            None = 0,    
            SingleNote = 1,
            LongNoteStart = 2,
            LongNoteEnd = 3,
            Effect1 = 4,
            Effect2 = 5,
            CameraEffect3 = 6
        }
    }
}

