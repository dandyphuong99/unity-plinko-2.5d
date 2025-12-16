using TikTokLiveSharp.Events.Objects;
using TikTokLiveUnity.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TikTokLiveUnity;

public class GiftRow : MonoBehaviour
    {
        #region Properties
        public TikTokGift ttGift { get; private set; }

        [SerializeField]
        private TMP_Text txtUserName;
        
        [SerializeField]
        private TMP_Text txtAmount;
       
        [SerializeField]
        private Image imgUserProfile;
        
        [SerializeField]
        private Image imgGiftIcon;
        #endregion

        #region Methods
        public void Init(TikTokGift gift)
        {
            ttGift = gift;
            ttGift.OnAmountChanged += AmountChanged;
            ttGift.OnStreakFinished += StreakFinished;
            txtUserName.text = $"{ttGift.Sender.UniqueId} sent a {ttGift.Gift.Name}!";
            txtAmount.text = $"{ttGift.Amount}x";
            RequestImage(imgUserProfile, ttGift.Sender.AvatarThumbnail);
            RequestImage(imgGiftIcon, ttGift.Gift.Image);
            // Run Streak-End for non-streakable gifts
            if (gift.StreakFinished)
                StreakFinished(gift, gift.Amount);
        }
        /// <summary>
        /// Deinitializes GiftRow
        /// </summary>
        private void OnDestroy()
        {
            gameObject.SetActive(false);
            if (ttGift == null)
                return;
            ttGift.OnAmountChanged -= AmountChanged;
            ttGift.OnStreakFinished -= StreakFinished;
        }
        /// <summary>
        /// Updates Gift-Amount if Amount Changed
        /// </summary>
        /// <param name="gift">Gift for Event</param>
        /// <param name="newAmount">New Amount</param>
        private void AmountChanged(TikTokGift gift, long change, long newAmount)
        {
            txtAmount.text = $"{newAmount}x";
            GiftChecker(gift.Gift.Name);
        }
        /// <summary>
        /// Called when GiftStreaks Ends. Starts Destruction-Timer
        /// </summary>
        /// <param name="gift">Gift for Event</param>
        /// <param name="finalAmount">Final Amount for Streak</param>
        private void StreakFinished(TikTokGift gift, long finalAmount)
        {
            AmountChanged(gift, 0, finalAmount);
            Destroy(gameObject, 2f);
        }
        
        private static void RequestImage(Image img, Picture picture)
        {
            Dispatcher.RunOnMainThread(() =>
            {
                if (TikTokLiveManager.Exists)
                    TikTokLiveManager.Instance.RequestSprite(picture, spr =>
                    {
                        if (img != null && img.gameObject != null && img.gameObject.activeInHierarchy)
                            img.sprite = spr;
                    });
            });
        }

        private void GiftChecker(string giftName)
        {
            Debug.Log($"Gift Received: {giftName}");
        }
        #endregion
    }
