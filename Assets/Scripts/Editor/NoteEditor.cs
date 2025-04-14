using UnityEditor;

[CustomEditor(typeof(Note))]
public class NoteEditor : Editor
{
    Note note;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        note = target as Note;
        if (note.messages.Length > 0)
        {
            for (int i = 0; i < note.messages.Length; i++)
            {
                EditorGUILayout.HelpBox(note.messages[i], MessageType.Info);
            }
        }
    }
}