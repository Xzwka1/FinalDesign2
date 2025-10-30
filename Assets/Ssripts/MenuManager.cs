using UnityEngine;
using UnityEngine.SceneManagement; // 1. ��ͧ������÷Ѵ��� �����������¹�ҡ��

public class MainMenu : MonoBehaviour
{
    // �ѧ��ѹ����Ѻ���� "�������"
    // ��ͧ�� public ��ҹ�� �����֧���ͧ���
    public void StartGame()
    {
        // "MyGameD2" ��� "�������" Scene ���ͧ�س (��ͧ�ç����)
        // (���ͪ��� DesignProject ������س�µ��)
        SceneManager.LoadScene("InGame");
    }

    // �ѧ��ѹ����Ѻ���� "�͡��"
    public void QuitGame()
    {
        // ����觹��лԴ�� (����繼�੾�е͹ Build ��������ҹ��)
        Debug.Log("QUIT GAME!");
        Application.Quit();
    }
}