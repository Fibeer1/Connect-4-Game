using UnityEngine;

public class GridColumn : Interactable
{
    [SerializeField] private int columnIndex;
    private TokenGrid tokenGrid;
    private RoundManager roundManager;
    private Player player;

    private void Start()
    {
        //1 instance of each of the scripts below will be available in the scene, so might as well make referencing automatic
        tokenGrid = FindAnyObjectByType<TokenGrid>();
        roundManager = FindAnyObjectByType<RoundManager>();
        player = FindAnyObjectByType<Player>();

        roundManager.OnTurnSwitch += OnTurnSwitch;
        TogglePreview(false);
    }

    private void OnTurnSwitch()
    {
        if (player.currentInteractable == this)
        {
            //Update the preview depending on whether the player can put tokens
            if (roundManager.currentPlayerTurn == 
                RoundManager.PlayerType.Player && 
                !IsPreviewActive())
            {
                TogglePreview(true);
            }
            else
            {
                TogglePreview(false);
            }
        }
    }

    public override void OnHoverEnter()
    {
        base.OnHoverEnter();
        //Do not activate the preview if it's not the player's turn or if the column is full
        if (tokenGrid.currentPlayer != RoundManager.PlayerType.Player || 
            roundManager.currentPlayerTurn != RoundManager.PlayerType.Player ||
            tokenGrid.IsColumnFull(columnIndex))
        {
            TogglePreview(false);
            return;
        }
        TogglePreview(true);
    }

    public override void OnHoverExit()
    {
        base.OnHoverExit();
        //If the preview has been activated, then it's the player's turn,
        //meaning there's no need to check if it is
        TogglePreview(false);
    }

    private void TogglePreview(bool shouldActivate)
    {
        tokenGrid.previewTokens[columnIndex].gameObject.SetActive(shouldActivate);
    }

    private bool IsPreviewActive()
    {
        return tokenGrid.previewTokens[columnIndex].gameObject.activeSelf;
    }

    public override void Interact()
    {
        base.Interact();
        if (!IsPreviewActive())
        {
            //If the preview is not being shown, then the player cannot put a token in this column
            return;
        }
        tokenGrid.AddTokenToColumn(columnIndex, 1);
        TogglePreview(false);
    }
}
