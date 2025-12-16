using System.Collections;
using TikTokLiveSharp.Client;
using TikTokLiveSharp.Events;
using TikTokLiveSharp.Events.Objects;
using TikTokLiveUnity.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TikTokLiveUnity;

public class TiktokExample : MonoBehaviour
    {
        #region Properties
        [Header("Settings")]
        [SerializeField]
        public float timeToLive = 3f;

        [Header("ScrollRects")]
    [SerializeField]
    private ScrollRect scrGift;

    [Header("StatusPanel")]
        [SerializeField]
        private TMP_Text txtStatusTitle;
        
        [SerializeField]
        private TMP_InputField ifHostId;
        
        [SerializeField]
        private TMP_InputField ifRoomId;
        
        [SerializeField]
        private Button btnConnect;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject rowPrefab;
        
        [SerializeField]
        private GiftRow giftRowPrefab;

        private TikTokLiveManager mgr => TikTokLiveManager.Instance;
        #endregion

        #region Methods
        #region Unity
        /// <summary>
        /// Initializes this Object
        /// </summary>
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
        /// <summary>
        /// Handler for Connect-Button
        /// </summary>
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
        /// <summary>
        /// Handler for Connection-Events. Updates StatusPanel
        /// </summary>
        private void ConnectStatusChange(TikTokLiveClient sender, bool e) => UpdateStatus();


    /// Khi có quà tặng mới
    private void OnGift(TikTokLiveClient sender, TikTokGift gift)
    {
        GiftRow instance = Instantiate(giftRowPrefab);
        instance.Init(gift);
        instance.transform.SetParent(scrGift.content, false);
        instance.transform.localScale = Vector3.one;
        instance.gameObject.SetActive(true);
    }

    private void CreateRow(ScrollRect targetScroll, Picture avatarPicture, string message)
        {
            GameObject instance = Instantiate(rowPrefab, targetScroll.content);
            Image img = instance.GetComponentInChildren<Image>();
            TMP_Text txt = instance.GetComponentInChildren<TMP_Text>();

            RequestImage(img, avatarPicture);
            txt.text = message;

            instance.transform.localScale = Vector3.one;
            instance.SetActive(true);

            Destroy(instance, timeToLive);
            //LimitScrollItems(targetScroll, 2);
        }


        /// Giới hạn số lượng mục con trong ScrollRect để tránh sử dụng quá nhiều bộ nhớ
        private void LimitScrollItems(ScrollRect scroll, int maxItems = 30)
        {
            while (scroll.content.childCount > maxItems)
            {
                Destroy(scroll.content.GetChild(0).gameObject);
                /// hoặc ReturnToPool nếu dùng pool
            }
        }


        /// Khi người dùng tham gia
        private void OnJoin(TikTokLiveClient sender, Join join)
        {
            string userId = join.User.UniqueId;
            Picture avatarPicture = join.User.AvatarThumbnail;
            ///
        }

        /// Khi người dùng thích
        private void OnLike(TikTokLiveClient sender, Like like)
        {
            string userId = like.Sender.UniqueId;
            Picture avatarPicture = like.Sender.AvatarThumbnail;
            ///
        }

        /// Khi có bình luận mới
        private void OnComment(TikTokLiveClient sender, Chat comment)
        {
            string userId = comment.Sender.UniqueId;
            Picture avatarPicture = comment.Sender.AvatarThumbnail;
            ///
        }

        private void RequestImage(Image img, Picture picture)
        {
            Dispatcher.RunOnMainThread(() =>
            {
                mgr.RequestSprite(picture, spr =>
                {
                    img.sprite = spr;
                });
            });
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
