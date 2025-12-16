using System.Collections;
using TikTokLiveSharp.Client;
using TikTokLiveSharp.Events;
using TikTokLiveSharp.Events.Objects;
using TikTokLiveUnity.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TikTokLiveUnity;
using System;
using System.Collections.Generic;

public class LiveManager : MonoBehaviour
{
    #region Properties
    [Header("Settings")]
    [SerializeField]
    public float timeToLive = 3f;

    [Header("StatusPanel")]
    [SerializeField]
    private TMP_Text txtStatusTitle;

    [SerializeField]
    private TMP_InputField ifHostId;

    [SerializeField]
    private TMP_InputField ifRoomId;

    [SerializeField]
    private Button btnConnect;

    [SerializeField] private Ball ballNamPrefab;
    [SerializeField] private Ball ballNuPrefab;

    private Transform spawnPoint;
    private Queue<(Picture avatar, Ball.Team team)> ballQueue = new Queue<(Picture, Ball.Team)>();
    private bool isProcessing = false;

    private TikTokLiveManager mgr => TikTokLiveManager.Instance;
    #endregion

    #region Methods
    private void Awake()
    {
        spawnPoint = ballNamPrefab.transform;
    }

    #region Unity
    /// <summary>
    /// Initializes this Object
    /// </summary>
    /// 
    private IEnumerator Start()
    {
        btnConnect.onClick.AddListener(OnClick_Connect);
        mgr.OnConnected += ConnectStatusChange;
        mgr.OnDisconnected += ConnectStatusChange;
        mgr.OnJoin += OnJoin;
        mgr.OnLike += OnLike;
        mgr.OnChatMessage += OnComment;
        mgr.OnGift += OnGift;
        for (int i = 0; i < 3; i++)
            yield return null; // Wait 3 frames in case Auto-Connect is enabled
        UpdateStatus();
    }


    /// <summary>
    /// Deinitializes this Object
    /// </summary>
    private void OnDestroy()
    {
        btnConnect.onClick.RemoveListener(OnClick_Connect);
        if (!TikTokLiveManager.Exists)
            return;
        mgr.OnConnected -= ConnectStatusChange;
        mgr.OnDisconnected -= ConnectStatusChange;
        mgr.OnJoin -= OnJoin;
        mgr.OnLike -= OnLike;
        mgr.OnChatMessage -= OnComment;
        mgr.OnGift -= OnGift;
    }
    #endregion

    #region Private
    private void OnClick_Connect()
    {
        bool connected = mgr.Connected;
        bool connecting = mgr.Connecting;
        if (connected || connecting)
            mgr.DisconnectFromLivestreamAsync();
        else
        {
            if (!string.IsNullOrEmpty(ifRoomId.text))
                mgr.ConnectToRoomAsync(ifRoomId.text, Debug.LogException);
            else
                mgr.ConnectToStreamAsync(ifHostId.text, Debug.LogException);
        }
        UpdateStatus();
        Invoke(nameof(UpdateStatus), 1.5f);
    }
    private void ConnectStatusChange(TikTokLiveClient sender, bool e) => UpdateStatus();


    private void CreateBall(Picture avatarPicture, Ball.Team team)
    {
        ballQueue.Enqueue((avatarPicture, team));
        Debug.Log("Queue size: " + ballQueue.Count);
        if (!isProcessing)
            StartCoroutine(ProcessBallQueue());
    }

   private IEnumerator ProcessBallQueue()
    {
        isProcessing = true;

        while (ballQueue.Count > 0)
        {
            var item = ballQueue.Dequeue();
            Ball prefab = (item.team == Ball.Team.Nam) ? ballNamPrefab : ballNuPrefab;

            Ball newBall = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            newBall.InitAvatar(item.avatar, item.team);
            newBall.gameObject.SetActive(true);
            newBall.EnableFall();

            // Delay 0.2s giữa các ball để không chồng nhau
            yield return new WaitForSeconds(0.2f);
        }

        isProcessing = false;
    }

    /// Khi có quà tặng mới
    private void OnGift(TikTokLiveClient sender, TikTokGift gift)
    {
        string userId = gift.Sender.UniqueId;
        Picture avatarPicture = gift.Sender.AvatarThumbnail;
        CreateBall(avatarPicture, Ball.Team.Nu);
    }


    /// Khi người dùng tham gia
    private void OnJoin(TikTokLiveClient sender, Join join)
    {
        string userId = join.User.UniqueId;
        Picture avatarPicture = join.User.AvatarThumbnail;
        Debug.Log("Join from " + userId);
        Ball.Team team = (UnityEngine.Random.value > 0.5f) ? Ball.Team.Nam : Ball.Team.Nu;
        CreateBall(avatarPicture, team);
        ///
    }

    /// Khi người dùng thích
    private void OnLike(TikTokLiveClient sender, Like like)
    {
        string userId = like.Sender.UniqueId;
        Picture avatarPicture = like.Sender.AvatarThumbnail;
        Debug.Log("Like from " + userId);
        CreateBall(avatarPicture, Ball.Team.Nu);
        ///
    }

    /// Khi có bình luận mới
    private void OnComment(TikTokLiveClient sender, Chat comment)
    {
        string userId = comment.Sender.UniqueId;
        Debug.Log("Comment from " + userId + " - " + comment.Message);
        Picture avatarPicture = comment.Sender.AvatarThumbnail;
        CreateBall(avatarPicture, Ball.Team.Nam);
        ///
    }

    private void UpdateStatus()
    {
        bool connected = mgr.Connected;
        bool connecting = mgr.Connecting;
        string currentId = string.IsNullOrWhiteSpace(mgr.HostName) ? mgr.RoomId : mgr.HostName;
        txtStatusTitle.text = connected ? "Đã kết nối: " + currentId : connecting ? "Đang kết nối..." : "Chưa kết nối";

        bool needConnect = !connected && !connecting;
        ifHostId.gameObject.SetActive(needConnect);
        ifRoomId.gameObject.SetActive(needConnect);
        btnConnect.GetComponentInChildren<TMP_Text>().text = connected ? "Ngắt kết nối" : connecting ? "Hủy" : "Kết nối";
    }
    #endregion
    #endregion
}
