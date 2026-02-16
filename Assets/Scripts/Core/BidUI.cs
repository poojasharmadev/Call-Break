using UnityEngine;

namespace Core
{
    public class BidUI : MonoBehaviour
    {
        public GameManager gameManager;
        public GameObject bidPanel;

        int selectedBid = 1;

        public void Open()
        {
            bidPanel.SetActive(true);
        }

        public void SetBid(int bid)
        {
            selectedBid = bid;
            Debug.Log("Selected Bid: " + selectedBid);
        }

        public void ConfirmBid()
        {
            bidPanel.SetActive(false);
            gameManager.OnHumanBidConfirmed(selectedBid);
        }
    }
}