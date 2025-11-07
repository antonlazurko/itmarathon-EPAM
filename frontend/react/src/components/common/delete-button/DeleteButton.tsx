import IconButton from "../icon-button/IconButton";
import type { DeleteButtonProps } from "./types";

const DeleteButton = ({ onDeleteButtonClick }: DeleteButtonProps) => (
  <div className="copy-button">
    <IconButton iconName="delete" color="green" onClick={onDeleteButtonClick} />
  </div>
);

export default DeleteButton;
