using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RhythmGame
{
    public class Enums
    {

        /// <summary>
        /// ���� ��ȭ�Ҷ� �����
        /// </summary>
        public enum Modifier
        {
            /// <summary>
            /// ����
            /// </summary>
            NONE,
            /// <summary>
            /// ����, ���
            /// </summary>
            POSITIVE,
            /// <summary>
            /// ����, ����
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

